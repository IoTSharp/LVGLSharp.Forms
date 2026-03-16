using LVGLSharp;
using LVGLSharp.Interop;
using System.Diagnostics;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

unsafe class Program
{
    static lv_obj_t* port_dropdown;
    static lv_obj_t* baud_dropdown;
    static lv_obj_t* ref_btn;
    static lv_obj_t* open_btn;
    static lv_obj_t* recv_textarea;
    static lv_obj_t* send_textarea;
    static lv_obj_t* send_btn;
    static lv_obj_t* clear_btn;
    static lv_obj_t* hex_switch;
    static IWindow? window;
    static SerialPort? serialPort;
    static List<string> serialPorts = [];
    static List<string> bauds = ["9600", "19200", "38400", "57600", "115200"];

    static lv_obj_t* root;
    static lv_group_t* key_inputGroup = null;
    static delegate* unmanaged[Cdecl]<lv_event_t*, void> SendTextAreaFocusCb = null;

    static void Main(string[] args)
    {
        window = PlatformWindowFactory.Create();
        window.Init();

        root = window.Root;
        key_inputGroup = window.KeyInputGroup;
        SendTextAreaFocusCb = window.SendTextAreaFocusCallback;

        InitUI();

        window.StartLoop(() => { });
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    static unsafe void RefButtonClick(lv_event_t* obj)
    {
        lv_event_code_t code = lv_event_get_code(obj);
        if (code == lv_event_code_t.LV_EVENT_CLICKED)
        {
            RefSerialPort();
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    static unsafe void OpenButtonClick(lv_event_t* e)
    {
        lv_event_code_t code = lv_event_get_code(e);
        if (code == lv_event_code_t.LV_EVENT_CLICKED)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
                fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes("打开串口"))
                    lv_label_set_text(lv_obj_get_child(open_btn, 0), utf8Ptr);
            }
            else
            {
                var portName = serialPorts[(int)GetSelectedIndex(port_dropdown)];
                var baudRateStr = bauds[(int)GetSelectedIndex(baud_dropdown)];
                if (string.IsNullOrEmpty(portName) || string.IsNullOrEmpty(baudRateStr))
                    return;

                serialPort = new SerialPort(portName, int.Parse(baudRateStr));
                try
                {
                    serialPort.Open();
                    fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes("关闭串口"))
                        lv_label_set_text(lv_obj_get_child(open_btn, 0), utf8Ptr);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"打开串口失败: {ex.Message}");
                }
            }
        }
    }

    static uint GetSelectedIndex(lv_obj_t* dropdown)
    {
        return lv_dropdown_get_selected(dropdown);
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    static unsafe void SendButtonClick(lv_event_t* e)
    {
        lv_event_code_t code = lv_event_get_code(e);
        if (code == lv_event_code_t.LV_EVENT_CLICKED)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                var sendText = Marshal.PtrToStringUTF8((nint)lv_textarea_get_text(send_textarea)) ?? "";
                if (!string.IsNullOrEmpty(sendText))
                {
                    try
                    {
                        serialPort.Write(sendText);
                        Thread.Sleep(300);

                        int bytesToRead = serialPort.BytesToRead;
                        byte[] buffer = new byte[bytesToRead];
                        serialPort.Read(buffer, 0, bytesToRead);

                        string text;
                        if (lv_obj_has_state(hex_switch, LV_STATE_CHECKED))
                        {
                            text = BitConverter.ToString(buffer).Replace("-", " ") + "\n";
                        }
                        else
                        {
                            text = Encoding.UTF8.GetString(buffer);
                        }

                        var currentText = Marshal.PtrToStringUTF8((nint)lv_textarea_get_text(recv_textarea)) ?? "";
                        string newText = currentText + text;
                        fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes(newText))
                            lv_textarea_set_text(recv_textarea, utf8Ptr);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"错误: {ex.Message}");
                    }
                }
            }
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    static unsafe void ClearButtonClick(lv_event_t* e)
    {
        lv_event_code_t code = lv_event_get_code(e);
        if (code == lv_event_code_t.LV_EVENT_CLICKED)
        {
            fixed (byte* utf8Ptr = Encoding.ASCII.GetBytes("\0"))
                lv_textarea_set_text(recv_textarea, utf8Ptr);
        }
    }

    static void RefSerialPort()
    {
        serialPorts = SerialPort.GetPortNames().ToList();
        if (serialPorts.Count > 0)
        {
            fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes(string.Join('\n', serialPorts)))
                lv_dropdown_set_options(port_dropdown, utf8Ptr);
        }
        else
        {
            lv_dropdown_clear_options(port_dropdown);
        } 
            
    }

    static void InitUI()
    {
        lv_obj_set_flex_flow(root, LV_FLEX_FLOW_COLUMN);
        lv_obj_set_style_pad_all(root, 10, 0);

        // 顶部工具行容器
        var toolbar = lv_obj_create(root);
        lv_obj_set_height(toolbar, 100);
        lv_obj_set_width(toolbar, 670);
        lv_obj_set_flex_flow(toolbar, LV_FLEX_FLOW_ROW);
        lv_obj_set_style_pad_gap(toolbar, 10, 0);

        // 串口号
        var port_label = lv_label_create(toolbar);
        fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes("串口:"))
            lv_label_set_text(port_label, utf8Ptr);
        lv_obj_set_height(port_label, 50);

        // 串口下拉
        port_dropdown = lv_dropdown_create(toolbar);
        RefSerialPort();
        lv_obj_set_width(port_dropdown, 150);
        lv_obj_set_height(port_dropdown, 50);

        // 刷新串口按钮
        ref_btn = lv_btn_create(toolbar);
        var ref_btn_label = lv_label_create(ref_btn);
        fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes("刷新串口"))
            lv_label_set_text(ref_btn_label, utf8Ptr);
        lv_obj_add_event(ref_btn, &RefButtonClick, lv_event_code_t.LV_EVENT_ALL, null);
        lv_obj_set_height(ref_btn_label, 20);

        // 波特率
        var baud_label = lv_label_create(toolbar);
        fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes("波特率:"))
            lv_label_set_text(baud_label, utf8Ptr);
        lv_obj_set_height(baud_label, 50);

        // 波特率下拉
        baud_dropdown = lv_dropdown_create(toolbar);
        fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes(string.Join('\n', bauds)))
            lv_dropdown_set_options(baud_dropdown, utf8Ptr);
        lv_obj_set_width(baud_dropdown, 150);
        lv_obj_set_height(baud_dropdown, 50);

        // 打开串口按钮
        open_btn = lv_btn_create(toolbar);
        var btn_label = lv_label_create(open_btn);
        fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes("打开串口"))
            lv_label_set_text(btn_label, utf8Ptr);
        lv_obj_add_event(open_btn, &OpenButtonClick, lv_event_code_t.LV_EVENT_ALL, null);
        lv_obj_set_height(btn_label, 20);

        // 接收区容器
        var recv_container = lv_obj_create(root);
        lv_obj_set_height(recv_container, 190);
        lv_obj_set_width(recv_container, 670);
        lv_obj_set_flex_flow(recv_container, LV_FLEX_FLOW_ROW);
        lv_obj_set_style_pad_gap(recv_container, 10, 0);

        // 接收区
        recv_textarea = lv_textarea_create(recv_container);
        if (key_inputGroup != null)
            lv_group_add_obj(key_inputGroup, recv_textarea);
        lv_obj_set_flex_grow(recv_textarea, 1);
        lv_obj_set_height(recv_textarea, 150);
        fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes("接收的数据..."))
            lv_textarea_set_placeholder_text(recv_textarea, utf8Ptr);

        // 清空按钮
        clear_btn = lv_btn_create(recv_container);
        var clear_label = lv_label_create(clear_btn);
        fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes("清空"))
            lv_label_set_text(clear_label, utf8Ptr);
        lv_obj_add_event(clear_btn, &ClearButtonClick, lv_event_code_t.LV_EVENT_ALL, null);
        lv_obj_set_height(clear_label, 30);

        // HEX显示
        hex_switch = lv_switch_create(recv_container);
        var switch_label = lv_label_create(recv_container);
        fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes("HEX模式"))
            lv_label_set_text(switch_label, utf8Ptr);
        lv_obj_set_height(switch_label, 50);

        // 发送区容器
        var send_container = lv_obj_create(root);
        lv_obj_set_height(send_container, 90);
        lv_obj_set_width(send_container, 670);
        lv_obj_set_flex_flow(send_container, LV_FLEX_FLOW_ROW);
        lv_obj_set_style_pad_gap(send_container, 10, 0);

        // 发送区
        send_textarea = lv_textarea_create(send_container);
        lv_obj_set_flex_grow(send_textarea, 1);
        fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes("输入的数据..."))
            lv_textarea_set_placeholder_text(send_textarea, utf8Ptr);
        if (SendTextAreaFocusCb != null)
            lv_obj_add_event_cb(send_textarea, SendTextAreaFocusCb, lv_event_code_t.LV_EVENT_FOCUSED, null);
        if (key_inputGroup != null)
            lv_group_add_obj(key_inputGroup, send_textarea);
        lv_obj_set_height(send_textarea, 50);

        window?.AttachTextInput(send_textarea);

        // 发送按钮
        send_btn = lv_btn_create(send_container);
        var send_label = lv_label_create(send_btn);
        fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes("发送"))
            lv_label_set_text(send_label, utf8Ptr);
        lv_obj_add_event(send_btn, &SendButtonClick, lv_event_code_t.LV_EVENT_ALL, null);
        lv_obj_set_height(send_label, 30);
    }
}