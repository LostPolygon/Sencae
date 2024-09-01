using ImGuiNET;

namespace Sencae.Desktop;

using static ImGui;
using unsafe ImGuiID = System.UInt32;

public class ManagedImGuiDemoWindow {
    // ID Stack Tool: Display UI
    public void Draw() {
               if (ImGui.TreeNode("Multi-Select (in a table)"))
        {
            static ImGuiSelectionBasicStorage selection;

            const int ITEMS_COUNT = 10000;
            ImGui.Text("Selection: %d/%d", selection.Size, ITEMS_COUNT);
            if (ImGui.BeginTable("##Basket", 2, ImGuiTableFlags.ScrollY | ImGuiTableFlags_RowBg | ImGuiTableFlags_BordersOuter))
            {
                ImGui.TableSetupColumn("Object");
                ImGui.TableSetupColumn("Action");
                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                ImGuiMultiSelectFlags flags = ImGuiMultiSelectFlags_ClearOnEscape | ImGuiMultiSelectFlags_BoxSelect1d;
                ImGuiMultiSelectIO* ms_io = ImGui.BeginMultiSelect(flags, selection.Size, ITEMS_COUNT);
                selection.ApplyRequests(ms_io);

                ImGuiListClipper clipper;
                clipper.Begin(ITEMS_COUNT);
                if (ms_io->RangeSrcItem != -1)
                    clipper.IncludeItemByIndex((int)ms_io->RangeSrcItem); // Ensure RangeSrc item is not clipped.
                while (clipper.Step())
                {
                    for (int n = clipper.DisplayStart; n < clipper.DisplayEnd; n++)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();
                        char label[64];
                        sprintf(label, "Object %05d: %s", n, ExampleNames[n % IM_ARRAYSIZE(ExampleNames)]);
                        bool item_is_selected = selection.Contains((ImGuiID)n);
                        ImGui.SetNextItemSelectionUserData(n);
                        ImGui.Selectable(label, item_is_selected, ImGuiSelectableFlags_SpanAllColumns | ImGuiSelectableFlags_AllowOverlap);
                        ImGui.TableNextColumn();
                        ImGui.SmallButton("hello");
                    }
                }

                ms_io = ImGui.EndMultiSelect();
                selection.ApplyRequests(ms_io);
                ImGui.EndTable();
            }
            ImGui.TreePop();
        }
    }
}
