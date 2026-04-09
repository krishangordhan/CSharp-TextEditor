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

    internal void Insert(int offset, string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (offset < 0 || offset > Length)
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be within [0, Length].");

        if (text.Length == 0)
            return;

        var addStart = _addBuffer.Length;
        _addBuffer.Append(text);
        var newPiece = new Piece(BufferType.Add, addStart, text.Length);

        if (_pieces.Count == 0)
        {
            _pieces.Add(newPiece);
            return;
        }

        var accumulated = 0;

        for (var i = 0; i < _pieces.Count; i++)
        {
            var piece = _pieces[i];
            var pieceEnd = accumulated + piece.Length;

            if (offset == accumulated)
            {
                _pieces.Insert(i, newPiece);
                return;
            }

            if (offset == pieceEnd)
            {
                _pieces.Insert(i + 1, newPiece);
                return;
            }

            if (offset > accumulated && offset < pieceEnd)
            {
                SplitPieceAndInsert(i, accumulated, offset, newPiece);
                return;
            }

            accumulated += piece.Length;
        }
    }

    private void SplitPieceAndInsert(int pieceIndex, int pieceStartOffset, int insertOffset, Piece newPiece)
    {
        var piece = _pieces[pieceIndex];
        var leftLength = insertOffset - pieceStartOffset;

        var leftPiece = piece with { Length = leftLength };
        var rightPiece = piece with { Start = piece.Start + leftLength, Length = piece.Length - leftLength };

        _pieces.RemoveAt(pieceIndex);
        _pieces.Insert(pieceIndex, rightPiece);
        _pieces.Insert(pieceIndex, newPiece);
        _pieces.Insert(pieceIndex, leftPiece);
    }

    private string GetBufferText(Piece piece) =>
        piece.Buffer == BufferType.Original
            ? _originalBuffer.Substring(piece.Start, piece.Length)
            : _addBuffer.ToString(piece.Start, piece.Length);
}
