using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using ImGuiNET;

namespace Sencae.Desktop;

public abstract unsafe class HexEditorWindowBase : AppWindow {
    public const int BytesPerRow = 16;
    public const int GapPerColumns = 8;

    public uint MaxAddress { get; }

    protected abstract byte GetData(uint address);
    protected abstract void SetData(uint address, byte data);

    private Vector2 _originalWindowPadding;

    protected HexEditorWindowBase(
        string id,
        string title,
        AppWindowContext context,
        uint maxAddress
    ) : base(id,
        title,
        context) {
        MaxAddress = maxAddress;
        Flags = ImGuiWindowFlags.NoScrollbar;
    }

    protected override void DrawWindow(float deltaTime) {
        _originalWindowPadding = ImGui.GetStyle().WindowPadding;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        base.DrawWindow(deltaTime);
        ImGui.PopStyleVar();
    }

    protected override void DrawContent(float deltaTime) {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, _originalWindowPadding);

        ImGui.BeginChild(
            "##Top",
            new Vector2(0, 35),
            false,
            ImGuiWindowFlags.AlwaysUseWindowPadding |
            ImGuiWindowFlags.NoScrollbar |
            ImGuiWindowFlags.NoBackground |
            ImGuiWindowFlags.NoDecoration
        );

        ImGui.Text("Memory");
        ImGui.Separator();

        ImGui.EndChild();

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 0)); // Tighten spacing
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.GrabMinSize, 30);
        ImGui.PushFont(Context.SharedResources.FontMonospacedNormal);

        ImGui.BeginChild(
            "##ScrollView",
            Vector2.Zero,
            false,
            ImGuiWindowFlags.AlwaysUseWindowPadding |
            ImGuiWindowFlags.AlwaysVerticalScrollbar |
            ImGuiWindowFlags.NoBackground |
            ImGuiWindowFlags.NoDecoration
        );

        ImGuiListClipper clipperRaw = new();
        ImGuiListClipperPtr clipper = new(&clipperRaw);

        clipper.Begin((int) (MaxAddress / BytesPerRow) + 1, ImGui.GetTextLineHeightWithSpacing());

        Span<byte> rowData = stackalloc byte[BytesPerRow];
        Span<byte> rowDataHex = stackalloc byte[BytesPerRow * 2];
        Span<byte> rowAddressBytes = stackalloc byte[sizeof(ushort)];
        Span<byte> rowAddressHex = stackalloc byte[1 + sizeof(ushort) * 2];
        rowAddressHex[^1] = (byte) ':';

        const string byteFormat = "%02X";
        int byteFormatUtf8ByteCount = Encoding.UTF8.GetByteCount(byteFormat);
        Span<byte> byteFormatUtf8Bytes = stackalloc byte[byteFormatUtf8ByteCount + 1];
        Encoding.UTF8.GetBytes(byteFormat, byteFormatUtf8Bytes);
        byteFormatUtf8Bytes[^1] = 0;

        byte* rowDataHexPtr = (byte*) Unsafe.AsPointer(ref rowDataHex[0]);
        byte* rowAddressHexPtr = (byte*) Unsafe.AsPointer(ref rowAddressHex[0]);
        byte* byteFormatUtf8BytesPtr = (byte*) Unsafe.AsPointer(ref byteFormatUtf8Bytes[0]);

        byte* emptyString = stackalloc byte[1] { 0 };

        while (clipper.Step()) {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++) {
                int rowAddress = i * BytesPerRow;
                for (int column = 0; column < BytesPerRow; column++) {
                    int address = rowAddress + column;
                    rowData[column] = GetData((uint) address);
                }

                BinaryPrimitives.WriteUInt16BigEndian(rowAddressBytes, (ushort) rowAddress);
                HexUtility.HexEncodeBytesUpperCase(rowAddressBytes, rowAddressHex);

                ImGuiNative.igTextUnformatted(rowAddressHexPtr, rowAddressHexPtr + rowAddressHex.Length);
                ImGui.SameLine();
                ImGui.Dummy(new Vector2(20, 0));
                ImGui.SameLine();

                ImGui.PushItemWidth(50);
                for (int column = 0; column < BytesPerRow; column++) {
                    bool isLastColumn = column == BytesPerRow - 1;

                    if (column != 0 && !isLastColumn && column % GapPerColumns == 0) {
                        ImGui.Dummy(new Vector2(30, 0));
                        ImGui.SameLine();
                    }

                    /*HexUtility.HexEncodeBytesUpperCase(rowData, rowDataHex);

                    byte* hexStart = rowDataHexPtr + column * 2;
                    byte* hexEnd = hexStart + 2;
                    ImGuiNative.igTextUnformatted(hexStart, hexEnd);*/

                    ImGui.Text($"{rowData[column]:X2}");

                    /*int cellAddress = rowAddress + column;
                    ImGui.PushID(cellAddress);

                    byte cellData = rowData[column];
                    byte cellStep = 0;
                    bool cellEdited = ImGuiNative.igInputScalar(
                        emptyString,
                        ImGuiDataType.U8,
                        &cellData,
                        null,
                        null,
                        byteFormatUtf8BytesPtr,
                        ImGuiInputTextFlags.CharsHexadecimal |
                        ImGuiInputTextFlags.EnterReturnsTrue |
                        ImGuiInputTextFlags.AutoSelectAll |
                        ImGuiInputTextFlags.NoHorizontalScroll |
                        ImGuiInputTextFlags.AlwaysOverwrite
                        //ImGuiInputTextFlags.CallbackAlways
                    ) > 0;
                    if (cellEdited) {
                        int address = rowAddress + column;
                        SetData((uint) address, cellData);
                    }
                    ImGui.PopID();*/

                    if (!isLastColumn) {
                        ImGui.SameLine();
                    }
                }

                ImGui.PopItemWidth();

                ImGui.SameLine();
                ImGui.Dummy(new Vector2(20, 0));
                ImGui.SameLine();

                ImGui.PushItemWidth(30);

                for (int column = 0; column < BytesPerRow; column++) {
                    bool isLastColumn = column == BytesPerRow - 1;

                    string dataText = rowData[column] switch {
                        0 => " ",
                        >= 32 and < 128 => new string((char) rowData[column], 1),
                        _ => "."
                    };

                    ImGui.TextUnformatted(dataText);

                    if (!isLastColumn) {
                        ImGui.SameLine();
                    }
                }

                ImGui.PopItemWidth();
            }
        }

        clipper.End();

        ImGui.EndChild();

        ImGui.PopStyleVar(4);
        ImGui.PopFont();
    }
}
