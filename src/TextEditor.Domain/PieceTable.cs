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

    internal void Delete(int offset, int length)
    {
        if (offset < 0 || offset > Length)
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be within [0, Length].");
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be non-negative.");
        if (offset + length > Length)
            throw new ArgumentOutOfRangeException(nameof(length), "Delete range exceeds document length.");
        if (length == 0)
            return;

        var deleteEnd = offset + length;
        var accumulated = 0;
        var i = 0;

        while (i < _pieces.Count)
        {
            var piece = _pieces[i];
            var pieceStart = accumulated;
            var pieceEnd = accumulated + piece.Length;

            if (pieceEnd <= offset)
            {
                accumulated += piece.Length;
                i++;
                continue;
            }

            if (pieceStart >= deleteEnd)
                break;

            if (pieceStart < offset && pieceEnd > deleteEnd)
            {
                SplitPieceForDeletion(i, pieceStart, offset, deleteEnd);
                return;
            }

            if (pieceStart < offset)
            {
                _pieces[i] = piece with { Length = offset - pieceStart };
                accumulated += piece.Length;
                i++;
            }
            else if (pieceEnd > deleteEnd)
            {
                var trimAmount = deleteEnd - pieceStart;
                _pieces[i] = piece with { Start = piece.Start + trimAmount, Length = piece.Length - trimAmount };
                break;
            }
            else
            {
                _pieces.RemoveAt(i);
                accumulated += piece.Length;
            }
        }
    }

    private void SplitPieceForDeletion(int pieceIndex, int pieceStart, int deleteStart, int deleteEnd)
    {
        var piece = _pieces[pieceIndex];
        var leftLength = deleteStart - pieceStart;
        var rightSkip = deleteEnd - pieceStart;

        var leftPiece = piece with { Length = leftLength };
        var rightPiece = piece with { Start = piece.Start + rightSkip, Length = piece.Length - rightSkip };

        _pieces.RemoveAt(pieceIndex);
        _pieces.Insert(pieceIndex, rightPiece);
        _pieces.Insert(pieceIndex, leftPiece);
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

    internal string GetText()
    {
        if (_pieces.Count == 0)
            return string.Empty;

        var sb = new StringBuilder(Length);
        foreach (var piece in _pieces)
            sb.Append(GetBufferSlice(piece, 0, piece.Length));
        return sb.ToString();
    }

    internal string GetRange(int offset, int length)
    {
        if (offset < 0 || offset > Length)
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be within [0, Length].");
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be non-negative.");
        if (offset + length > Length)
            throw new ArgumentOutOfRangeException(nameof(length), "Range exceeds document length.");
        if (length == 0)
            return string.Empty;

        var rangeEnd = offset + length;
        var accumulated = 0;
        var sb = new StringBuilder(length);

        foreach (var piece in _pieces)
        {
            var pieceStart = accumulated;
            var pieceEnd = accumulated + piece.Length;
            accumulated += piece.Length;

            if (pieceEnd <= offset)
                continue;

            if (pieceStart >= rangeEnd)
                break;

            var clipStart = Math.Max(pieceStart, offset) - pieceStart;
            var clipEnd = Math.Min(pieceEnd, rangeEnd) - pieceStart;

            sb.Append(GetBufferSlice(piece, clipStart, clipEnd - clipStart));
        }

        return sb.ToString();
    }

    private string GetBufferSlice(Piece piece, int start, int length) =>
        piece.Buffer == BufferType.Original
            ? _originalBuffer.Substring(piece.Start + start, length)
            : _addBuffer.ToString(piece.Start + start, length);
}
