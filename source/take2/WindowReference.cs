using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Godot;

public partial class WindowReference : Node
{
    // TODO: remove this debug static
    public static WindowReference Instance;

    // Delegates
    private delegate IntPtr Proc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    public delegate void PixelSpaceChange(Vector2I size);
    public delegate void AspectChange(EAspect aspect, Vector2I size);
    public delegate void TextureChange(ImageTexture texture);
    private Proc _Proc;
    public PixelSpaceChange sizeChange;
    public AspectChange aspectChange;    
    public PixelSpaceChange positionChange;
    public TextureChange textureCapture;
    // Fields
    [Export] public string WindowTitle; // TODO: return to non-debug state no decorator and private
    private IntPtr _WinHandle;
    private IntPtr _WinProcOld;
    private GDICapture _Capture;
    private Rect2I _Rect;
    private EAspect _Aspect;

    private const double _CaptureInterval = 1d / 25d;
    private double _ElapsedInterval;
    public static EAspect Aspect(Vector2I size)
    {
        float a = size.X / size.Y;
        if (a < 1f) return EAspect.Portrait;
        else if (a > 1f) return EAspect.Landscape;
        else return EAspect.Square;
    }
    public WindowReference()
    {
        Instance = this;        // TODO: remove this debug global
        _Capture = new GDICapture();
        _ElapsedInterval = 0d;
    }

    public override void _Ready()
    {
        _WinHandle = FindWindow(null, WindowTitle);
        if (_WinHandle == IntPtr.Zero)
        {
            throw new ArgumentException($"No window exists with the title \"{WindowTitle}\"");
        }

        _Proc += OnProc;
        _WinProcOld = SetWindowLong(_WinHandle, PROC_INDEX, Marshal.GetFunctionPointerForDelegate(_Proc));
        _Capture = new GDICapture();
        GetWindowRect(_WinHandle, out RECT win_rect);
        _Rect = new(
            new(win_rect.Left, win_rect.Right),
            new(win_rect.Right - win_rect.Left, win_rect.Bottom - win_rect.Top)
        );
        _Aspect = Aspect(_Rect.Size);

        // // =============== DEBGGING HOOK ====================
        // Process[] processes = Process.GetProcessesByName(WindowTitle);

        // if (processes.Length == 0)
        // {
        //     Console.WriteLine("LDPlayer9 process not found.");
        //     return;
        // }

        // // Get the process ID of LDPlayer9
        // int targetProcessId = processes[0].Id;

        // // Hook injection
        // try
        // {
        //     // Start the hook in the target process
        //     RemoteHooking.Inject(
        //         targetProcessId,               // ID of the process to inject into
        //         typeof(DirectXHook).Assembly.Location, // Path to the hooking assembly
        //         typeof(DirectXHook).Assembly.Location, // Path to the hooking assembly (again for injected process)
        //         null                          // Any parameters to pass
        //     );

        //     Console.WriteLine("Hook injected successfully into process ID: " + targetProcessId);
        // }
        // catch (Exception ex)
        // {
        //     Console.WriteLine("Error: " + ex.Message);
        // }



        CallDeferred(WindowReference.MethodName.DeferredReady);
    }

    public void DeferredReady()
    {
        aspectChange?.Invoke(_Aspect, _Rect.Size);
        sizeChange?.Invoke(_Rect.Position);
    }

    public override void _Process(double delta)
    {
        _ElapsedInterval += delta;
        if (_ElapsedInterval >= _CaptureInterval)
        {
            var texture = _Capture.Capture(_WinHandle, _Rect);
            textureCapture?.Invoke(texture);
            _ElapsedInterval -= _CaptureInterval;
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();
    }

    public void _Free()
    {
        SetWindowLong(_WinHandle, PROC_INDEX, _WinProcOld);     // Restore original window procedure
    }

    protected IntPtr OnProc(IntPtr winHandle, uint eventId, IntPtr wParam, IntPtr lParam)
    {
        switch (eventId)
        {
            case WM_SIZE:
                int width = (int)lParam & 0xFFFF;            // low = X
                int height = ((int)lParam >> 16) & 0xFFFF;   // hi  = Y
                var new_size = new Vector2I(width, height);
                EAspect new_aspect = Aspect(new_size);
                if (new_aspect != _Aspect)
                    aspectChange?.Invoke(new_aspect, new_size);
                else
                    sizeChange?.Invoke(new_size);
                _Aspect = new_aspect;
                _Rect.Size = new_size;
                Console.WriteLine("Window resized");
                break;

            case WM_MOVE:
                int x = (int)lParam & 0xFFFF;           // low = X
                int y = ((int)lParam >> 16) & 0xFFFF;   // hi  = Y
                var new_pos = new Vector2I(x, y);
                positionChange?.Invoke(new_pos);
                _Rect.Position = new_pos;
                Console.WriteLine("Window moved");
                break;
            
            default:
                GD.Print($"Unhandled eventID: {eventId}");
                break;
        }
        return DefWindowProc(winHandle, eventId, wParam, lParam);
    }
        
    private IntPtr MakeLParam(Vector2 relativePos)
    {
        GD.Print($"---Making lParam---");
        GD.Print($"Rel Pos: {relativePos}");
        GD.Print($"Window Size: {_Rect.Size}");
        int x = Mathf.RoundToInt(relativePos.X * _Rect.Size.X);
        int y = Mathf.RoundToInt(relativePos.Y * _Rect.Size.Y);
        GD.Print($"WindowSpace Position: ({x}, {y})");
        return (IntPtr)((y << 16) | (x & 0xFFFF));
    }   
    
    public void SendMouseMove(Vector2 pos)
    {
        SendMessage(_WinHandle, WM_MOUSEMOVE, IntPtr.Zero, MakeLParam(pos));
    }

    public void SendMouseDown(Vector2 pos)
    {
        SendMessage(_WinHandle, WM_LBUTTONDOWN, (IntPtr)0x0001, MakeLParam(pos));
    }

    public void SendMouseUp(Vector2 pos)
    {
        SendMessage(_WinHandle, WM_LBUTTONUP, (IntPtr)0x0001, MakeLParam(pos));
    }
}
