using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Hooking;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace WindowLocker
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "Window Locker";

        private const string commandName = "/windowlocker";
        internal string lastMovedWindowName = "";

        private DalamudPluginInterface pi;
        internal Configuration config;
        private PluginUI ui;

        private IntPtr setUiPositionAddress = IntPtr.Zero;
        private delegate IntPtr SetUiPositionDelegate(IntPtr _this, IntPtr uiObject, ulong y);
        private Hook<SetUiPositionDelegate> setUiPositionHook;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pi = pluginInterface;


            setUiPositionAddress = this.pi.TargetModuleScanner.ScanText("40 53 48 83 EC 20 80 A2 ?? ?? ?? ?? ??");
            setUiPositionHook = new Hook<SetUiPositionDelegate>(setUiPositionAddress, new SetUiPositionDelegate(SetUiPositionDetour));
            setUiPositionHook.Enable();

            this.config = this.pi.GetPluginConfig() as Configuration ?? new Configuration();
            this.config.Initialize(this.pi);

            this.ui = new PluginUI(this);

            this.pi.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "A useful message to display in /xlhelp"
            });


            this.pi.UiBuilder.OnBuildUi += DrawUI;
            this.pi.UiBuilder.OnOpenConfigUi += (sender, args) => DrawConfigUI();
        }

        private unsafe IntPtr SetUiPositionDetour(IntPtr _this, IntPtr uiObject, ulong a3)
        {
            string windowName = Marshal.PtrToStringAnsi(uiObject + 8);
            PluginLog.Information($"windowName: {windowName}");
            lastMovedWindowName = windowName;
            if (this.config.lockedWindows.Contains(windowName))
                return IntPtr.Zero;
            return setUiPositionHook.Original(_this, uiObject, a3);
        }

        public void Dispose()
        {
            setUiPositionHook.Disable();
            this.ui.Dispose();

            this.pi.CommandManager.RemoveHandler(commandName);
            this.pi.Dispose();
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            this.ui.Visible = true;
        }

        private void DrawUI()
        {
            this.ui.Draw();
        }

        private void DrawConfigUI()
        {
            this.ui.Visible = true;
        }
    }
}
