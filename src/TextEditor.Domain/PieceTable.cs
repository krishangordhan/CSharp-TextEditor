using System.Text;

namespace TextEditor.Domain;

internal sealed class PieceTable
{
    private readonly string _originalBuffer;
    private readonly StringBuilder _addBuffer = new();
    private readonly List<Piece> _pieces = [];

    internal PieceTable(string originalText)
    {
        _originalBuffer = originalText;

        if (originalText.Length > 0)
            _pieces.Add(new Piece(BufferType.Original, 0, originalText.Length));
    }

    internal int Length => _pieces.Sum(p => p.Length);

    private string GetBufferText(Piece piece) =>
        piece.Buffer == BufferType.Original
            ? _originalBuffer.Substring(piece.Start, piece.Length)
            : _addBuffer.ToString(piece.Start, piece.Length);
}

