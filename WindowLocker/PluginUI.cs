using ImGuiNET;
using System;
using System.Numerics;

namespace WindowLocker
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    class PluginUI : IDisposable
    {
        private Plugin plugin;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }


        // passing in the image here just for simplicity
        public PluginUI(Plugin _plugin)
        {
            this.plugin = _plugin;
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            DrawWindow();
        }


        public void DrawWindow()
        {
            if (!visible)
            {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(232, 500), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("Window Locker", ref this.visible))
            {
                ImGui.Text("Last Moved Window:");
                ImGui.Text(this.plugin.lastMovedWindowName);
                ImGui.SameLine(ImGui.GetColumnWidth() - 20);
                if (ImGui.Button("Lock")) {
                    string windowName = this.plugin.lastMovedWindowName;
                    if (windowName!= "" && !this.plugin.config.lockedWindows.Contains(windowName))
                    {
                        this.plugin.config.lockedWindows.Add(windowName);
                        this.plugin.config.Save();
                    }
                }
                ImGui.Separator();
                ImGui.Text("Locked Windows:");
                foreach(var name in this.plugin.config.lockedWindows)
                {
                    if (ImGui.Selectable(name))
                    {
                        this.plugin.config.lockedWindows.Remove(name);
                        this.plugin.config.Save();
                        break;
                    }
                }
            }
            ImGui.End();
        }
    }
}
