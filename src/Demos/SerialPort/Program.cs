using LVGLSharp;
using LVGLSharp.Interop;
using LVGLSharp.Runtime.Windows;
using LVGLSharp.Runtime.Linux;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

unsafe class Program
{
    static lv_obj_t* portDropdown;
    static lv_obj_t* baudDropdown;
    static lv_obj_t* refreshButton;
    static lv_obj_t* openButton;
    static lv_obj_t* receiveTextArea;
    static lv_obj_t* sendTextArea;
    static lv_obj_t* sendButton;
    static lv_obj_t* clearButton;
    static lv_obj_t* hexSwitch;
    static IView? view;
    static SerialPort? serialPort;
    static List<string> serialPorts = [];
    static List<string> bauds = ["9600", "19200", "38400", "57600", "115200"];

    static lv_obj_t* rootObject;
    static lv_group_t* keyInputGroup = null;
    static delegate* unmanaged[Cdecl]<lv_event_t*, void> sendTextAreaFocusCallback = null;

    static void Main(string[] args)
    {
        view = CreateView();
        view.Open();

        rootObject = view.Root;
        keyInputGroup = view.KeyInputGroup;
        sendTextAreaFocusCallback = view.SendTextAreaFocusCallback;

        InitUI();

        view.RunLoop(() => { });
    }

    static IView CreateView()
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            return new Win32View("SerialPort", 700, 360);
        }
        else
        {
            return new LinuxView("SerialPort", 700, 360);
        }
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
                    lv_label_set_text(lv_obj_get_child(openButton, 0), utf8Ptr);
            }
            else
            {
                var portName = serialPorts[(int)GetSelectedIndex(portDropdown)];
                var baudRateStr = bauds[(int)GetSelectedIndex(baudDropdown)];
                if (string.IsNullOrEmpty(portName) || string.IsNullOrEmpty(baudRateStr))
                    return;

                serialPort = new SerialPort(portName, int.Parse(baudRateStr));
                try
                {
                    serialPort.Open();
                    fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes("关闭串口"))
                        lv_label_set_text(lv_obj_get_child(openButton, 0), utf8Ptr);
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
                var sendText = Marshal.PtrToStringUTF8((nint)lv_textarea_get_text(sendTextArea)) ?? "";
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
                        if (lv_obj_has_state(hexSwitch, LV_STATE_CHECKED))
                        {
                            text = BitConverter.ToString(buffer).Replace("-", " ") + "\n";
                        }
                        else
                        {
                            text = Encoding.UTF8.GetString(buffer);
                        }

                        var currentText = Marshal.PtrToStringUTF8((nint)lv_textarea_get_text(receiveTextArea)) ?? "";
                        string newText = currentText + text;
                        fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes(newText))
                            lv_textarea_set_text(receiveTextArea, utf8Ptr);
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
                lv_textarea_set_text(receiveTextArea, utf8Ptr);
        }
    }

    static void RefSerialPort()
    {
        serialPorts = SerialPort.GetPortNames().ToList();
        if (serialPorts.Count > 0)
        {
            fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes(string.Join('\n', serialPorts)))
                lv_dropdown_set_options(portDropdown, utf8Ptr);
        }
        else
        {
            lv_dropdown_clear_options(portDropdown);
        } 
            
    }

    static void InitUI()
    {
        lv_obj_set_flex_flow(rootObject, LV_FLEX_FLOW_COLUMN);
        lv_obj_set_style_pad_all(rootObject, 10, 0);

        // 顶部工具行容器
        var toolbar = lv_obj_create(rootObject);
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
        portDropdown = lv_dropdown_create(toolbar);
        RefSerialPort();
        lv_obj_set_width(portDropdown, 150);
        lv_obj_set_height(portDropdown, 50);

        // 刷新串口按钮
        refreshButton = lv_btn_create(toolbar);
        var ref_btn_label = lv_label_create(refreshButton);
        fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes("刷新串口"))
            lv_label_set_text(ref_btn_label, utf8Ptr);
        lv_obj_add_event(refreshButton, &RefButtonClick, lv_event_code_t.LV_EVENT_ALL, null);
        lv_obj_set_height(ref_btn_label, 20);

        // 波特率
        var baud_label = lv_label_create(toolbar);
        fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes("波特率:"))
            lv_label_set_text(baud_label, utf8Ptr);
        lv_obj_set_height(baud_label, 50);

        // 波特率下拉
        baudDropdown = lv_dropdown_create(toolbar);
        fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes(string.Join('\n', bauds)))
            lv_dropdown_set_options(baudDropdown, utf8Ptr);
        lv_obj_set_width(baudDropdown, 150);
        lv_obj_set_height(baudDropdown, 50);

        // 打开串口按钮
        openButton = lv_btn_create(toolbar);
        var btn_label = lv_label_create(openButton);
        fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes("打开串口"))
            lv_label_set_text(btn_label, utf8Ptr);
        lv_obj_add_event(openButton, &OpenButtonClick, lv_event_code_t.LV_EVENT_ALL, null);
        lv_obj_set_height(btn_label, 20);

        // 接收区容器
        var recv_container = lv_obj_create(rootObject);
        lv_obj_set_height(recv_container, 190);
        lv_obj_set_width(recv_container, 670);
        lv_obj_set_flex_flow(recv_container, LV_FLEX_FLOW_ROW);
        lv_obj_set_style_pad_gap(recv_container, 10, 0);

        // 接收区
        receiveTextArea = lv_textarea_create(recv_container);
        if (keyInputGroup != null)
            lv_group_add_obj(keyInputGroup, receiveTextArea);
        lv_obj_set_flex_grow(receiveTextArea, 1);
        lv_obj_set_height(receiveTextArea, 150);
        fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes("接收的数据..."))
            lv_textarea_set_placeholder_text(receiveTextArea, utf8Ptr);

        // 清空按钮
        clearButton = lv_btn_create(recv_container);
        var clear_label = lv_label_create(clearButton);
        fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes("清空"))
            lv_label_set_text(clear_label, utf8Ptr);
        lv_obj_add_event(clearButton, &ClearButtonClick, lv_event_code_t.LV_EVENT_ALL, null);
        lv_obj_set_height(clear_label, 30);

        // HEX显示
        hexSwitch = lv_switch_create(recv_container);
        var switch_label = lv_label_create(recv_container);
        fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes("HEX模式"))
            lv_label_set_text(switch_label, utf8Ptr);
        lv_obj_set_height(switch_label, 50);

        // 发送区容器
        var send_container = lv_obj_create(rootObject);
        lv_obj_set_height(send_container, 90);
        lv_obj_set_width(send_container, 670);
        lv_obj_set_flex_flow(send_container, LV_FLEX_FLOW_ROW);
        lv_obj_set_style_pad_gap(send_container, 10, 0);

        // 发送区
        sendTextArea = lv_textarea_create(send_container);
        lv_obj_set_flex_grow(sendTextArea, 1);
        fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes("输入的数据..."))
            lv_textarea_set_placeholder_text(sendTextArea, utf8Ptr);
        if (sendTextAreaFocusCallback != null)
            lv_obj_add_event_cb(sendTextArea, sendTextAreaFocusCallback, lv_event_code_t.LV_EVENT_FOCUSED, null);
        if (keyInputGroup != null)
            lv_group_add_obj(keyInputGroup, sendTextArea);
        lv_obj_set_height(sendTextArea, 50);

        view?.RegisterTextInput(sendTextArea);

        // 发送按钮
        sendButton = lv_btn_create(send_container);
        var send_label = lv_label_create(sendButton);
        fixed (byte* utf8Ptr = Encoding.UTF8.GetBytes("发送"))
            lv_label_set_text(send_label, utf8Ptr);
        lv_obj_add_event(sendButton, &SendButtonClick, lv_event_code_t.LV_EVENT_ALL, null);
        lv_obj_set_height(send_label, 30);
    }
}