using Shouldly;
using TextEditor.Domain;

namespace TextEditor.Domain.UnitTests;

public sealed class PieceTableTests
{
    [Fact]
    public void Insert_IntoEmptyDocument_UpdatesLength()
    {
        // Arrange
        var pieceTable = new PieceTable(string.Empty);

        // Act
        pieceTable.Insert(0, "hello");

        // Assert
        pieceTable.Length.ShouldBe(5);
    }

    [Fact]
    public void Insert_AtDocumentStart_UpdatesLength()
    {
        // Arrange
        var pieceTable = new PieceTable("World");

        // Act
        pieceTable.Insert(0, "Hello ");

        // Assert
        pieceTable.Length.ShouldBe(11);
    }

    [Fact]
    public void Insert_MultipleSequentialInserts_AccumulatesLength()
    {
        // Arrange
        var pieceTable = new PieceTable(string.Empty);

        // Act
        pieceTable.Insert(0, "Hello");
        pieceTable.Insert(5, " World");

        // Assert
        pieceTable.Length.ShouldBe(11);
    }

    [Fact]
    public void Insert_EmptyString_DoesNotChangeLength()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello");

        // Act
        pieceTable.Insert(0, string.Empty);

        // Assert
        pieceTable.Length.ShouldBe(5);
    }

    [Fact]
    public void Insert_NegativeOffset_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello");

        // Act
        var act = () => pieceTable.Insert(-1, "x");

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Insert_OffsetBeyondLength_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello");

        // Act
        var act = () => pieceTable.Insert(6, "x");

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Delete_ZeroLength_DoesNotChangeLength()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello");

        // Act
        pieceTable.Delete(0, 0);

        // Assert
        pieceTable.Length.ShouldBe(5);
    }


    [Fact]
    public void Delete_FromMiddle_DecreasesLength()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello");

        // Act
        pieceTable.Delete(1, 3);

        // Assert
        pieceTable.Length.ShouldBe(2);
    }

    [Fact]
    public void Delete_AcrossMultiplePieces_DecreasesLength()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello");
        pieceTable.Insert(5, " World");

        // Act
        pieceTable.Delete(3, 5);

        // Assert
        pieceTable.Length.ShouldBe(6);
    }

    [Fact]
    public void Delete_NegativeOffset_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello");

        // Act
        var act = () => pieceTable.Delete(-1, 1);

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Delete_NegativeLength_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello");

        // Act
        var act = () => pieceTable.Delete(0, -1);

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Delete_RangeExceedsDocumentLength_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello");

        // Act
        var act = () => pieceTable.Delete(3, 5);

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetText_EmptyDocument_ReturnsEmptyString()
    {
        // Arrange
        var pieceTable = new PieceTable(string.Empty);

        // Act
        var result = pieceTable.GetText();

        // Assert
        result.ShouldBe(string.Empty);
    }

    [Fact]
    public void GetText_OriginalTextOnly_ReturnsOriginalText()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello World");

        // Act
        var result = pieceTable.GetText();

        // Assert
        result.ShouldBe("Hello World");
    }

    [Fact]
    public void GetText_AfterInsertAtMiddle_ReturnsCorrectText()
    {
        // Arrange
        var pieceTable = new PieceTable("Helo");

        // Act
        pieceTable.Insert(3, "l");
        var result = pieceTable.GetText();

        // Assert
        result.ShouldBe("Hello");
    }

    [Fact]
    public void GetText_AfterDeleteFromMiddle_ReturnsCorrectText()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello");

        // Act
        pieceTable.Delete(1, 3);
        var result = pieceTable.GetText();

        // Assert
        result.ShouldBe("Ho");
    }

    [Fact]
    public void GetText_AfterMultipleOperations_ReturnsCorrectText()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello World");

        // Act
        pieceTable.Delete(5, 6);
        pieceTable.Insert(5, " Everyone");
        var result = pieceTable.GetText();

        // Assert
        result.ShouldBe("Hello Everyone");
    }

    [Fact]
    public void GetRange_ZeroLength_ReturnsEmptyString()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello World");

        // Act
        var result = pieceTable.GetRange(0, 0);

        // Assert
        result.ShouldBe(string.Empty);
    }


    [Fact]
    public void GetRange_AcrossPieceBoundary_ReturnsCorrectSubstring()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello");
        pieceTable.Insert(5, " World");

        // Act
        var result = pieceTable.GetRange(3, 5);

        // Assert
        result.ShouldBe("lo Wo");
    }

    [Fact]
    public void GetRange_NegativeOffset_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello");

        // Act
        var act = () => pieceTable.GetRange(-1, 1);

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetRange_RangeExceedsDocumentLength_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello");

        // Act
        var act = () => pieceTable.GetRange(3, 5);

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Insert_AtStart_ProducesCorrectText()
    {
        // Arrange
        var pieceTable = new PieceTable("World");

        // Act
        pieceTable.Insert(0, "Hello ");

        // Assert
        pieceTable.GetText().ShouldBe("Hello World");
    }

    [Fact]
    public void Insert_AtEnd_ProducesCorrectText()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello");

        // Act
        pieceTable.Insert(5, " World");

        // Assert
        pieceTable.GetText().ShouldBe("Hello World");
    }

    [Fact]
    public void Insert_IntoEmptyDocument_ProducesCorrectText()
    {
        // Arrange
        var pieceTable = new PieceTable(string.Empty);

        // Act
        pieceTable.Insert(0, "Hello");

        // Assert
        pieceTable.GetText().ShouldBe("Hello");
    }

    [Fact]
    public void GetRange_WithinOriginalBuffer_ReturnsCorrectSubstring()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello World");

        // Act
        var result = pieceTable.GetRange(6, 5);

        // Assert
        result.ShouldBe("World");
    }

    [Fact]
    public void GetText_AfterInsertAtStart_ReturnsCorrectText()
    {
        // Arrange
        var pieceTable = new PieceTable("World");

        // Act
        pieceTable.Insert(0, "Hello ");
        var result = pieceTable.GetText();

        // Assert
        result.ShouldBe("Hello World");
    }

    [Fact]
    public void GetText_AfterInsertAtEnd_ReturnsCorrectText()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello");

        // Act
        pieceTable.Insert(5, " World");
        var result = pieceTable.GetText();

        // Assert
        result.ShouldBe("Hello World");
    }

    [Fact]
    public void GetText_AfterSequentialInsertsAndDelete_ReturnsCorrectText()
    {
        // Arrange
        var pieceTable = new PieceTable(string.Empty);

        // Act
        pieceTable.Insert(0, "Hello");
        pieceTable.Insert(5, " World");
        pieceTable.Delete(5, 6);
        pieceTable.Insert(5, " Everyone");
        var result = pieceTable.GetText();

        // Assert
        result.ShouldBe("Hello Everyone");
    }

    [Fact]
    public void Delete_EntireDocument_ProducesEmptyText()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello");

        // Act
        pieceTable.Delete(0, 5);

        // Assert
        pieceTable.GetText().ShouldBe(string.Empty);
    }

    [Fact]
    public void Delete_AtStart_ProducesCorrectText()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello World");

        // Act
        pieceTable.Delete(0, 6);

        // Assert
        pieceTable.GetText().ShouldBe("World");
    }

    [Fact]
    public void Delete_AtEnd_ProducesCorrectText()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello World");

        // Act
        pieceTable.Delete(5, 6);

        // Assert
        pieceTable.GetText().ShouldBe("Hello");
    }

    [Fact]
    public void LineCount_EmptyDocument_ReturnsOne()
    {
        // Arrange
        var pieceTable = new PieceTable(string.Empty);

        // Act
        var result = pieceTable.LineCount;

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public void LineCount_SingleLineDocument_ReturnsOne()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello World");

        // Act
        var result = pieceTable.LineCount;

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public void LineCount_MultiLineDocument_ReturnsCorrectCount()
    {
        // Arrange
        var pieceTable = new PieceTable("Line1\nLine2\nLine3");

        // Act
        var result = pieceTable.LineCount;

        // Assert
        result.ShouldBe(3);
    }

    [Fact]
    public void LineCount_TrailingNewline_CountsExtraEmptyLine()
    {
        // Arrange
        var pieceTable = new PieceTable("Line1\nLine2\n");

        // Act
        var result = pieceTable.LineCount;

        // Assert
        result.ShouldBe(3);
    }

    [Fact]
    public void LineCount_AfterInsertingNewline_IncrementsCount()
    {
        // Arrange
        var pieceTable = new PieceTable("HelloWorld");

        // Act
        pieceTable.Insert(5, "\n");
        var result = pieceTable.LineCount;

        // Assert
        result.ShouldBe(2);
    }

    [Fact]
    public void LineCount_AfterDeletingNewline_DecrementsCount()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello\nWorld");

        // Act
        pieceTable.Delete(5, 1);
        var result = pieceTable.LineCount;

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public void GetLineText_SingleLineDocument_ReturnsFullText()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello");

        // Act
        var result = pieceTable.GetLineText(0);

        // Assert
        result.ShouldBe("Hello");
    }

    [Fact]
    public void GetLineText_FirstLine_ReturnsTextBeforeNewline()
    {
        // Arrange
        var pieceTable = new PieceTable("Line1\nLine2");

        // Act
        var result = pieceTable.GetLineText(0);

        // Assert
        result.ShouldBe("Line1");
    }

    [Fact]
    public void GetLineText_LastLine_ReturnsTextAfterFinalNewline()
    {
        // Arrange
        var pieceTable = new PieceTable("Line1\nLine2");

        // Act
        var result = pieceTable.GetLineText(1);

        // Assert
        result.ShouldBe("Line2");
    }

    [Fact]
    public void GetLineText_MiddleLine_ReturnsCorrectSegment()
    {
        // Arrange
        var pieceTable = new PieceTable("Line1\nLine2\nLine3");

        // Act
        var result = pieceTable.GetLineText(1);

        // Assert
        result.ShouldBe("Line2");
    }

    [Fact]
    public void GetLineText_EmptyLine_ReturnsEmptyString()
    {
        // Arrange
        var pieceTable = new PieceTable("Line1\n\nLine3");

        // Act
        var result = pieceTable.GetLineText(1);

        // Assert
        result.ShouldBe(string.Empty);
    }

    [Fact]
    public void GetLineText_TrailingNewlineLastLine_ReturnsEmptyString()
    {
        // Arrange
        var pieceTable = new PieceTable("Line1\n");

        // Act
        var result = pieceTable.GetLineText(1);

        // Assert
        result.ShouldBe(string.Empty);
    }

    [Fact]
    public void GetLineText_EmptyDocument_ReturnsEmptyString()
    {
        // Arrange
        var pieceTable = new PieceTable(string.Empty);

        // Act
        var result = pieceTable.GetLineText(0);

        // Assert
        result.ShouldBe(string.Empty);
    }

    [Fact]
    public void GetLineText_NegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello");

        // Act
        var act = () => pieceTable.GetLineText(-1);

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetLineText_IndexEqualToLineCount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello\nWorld");

        // Act
        var act = () => pieceTable.GetLineText(2);

        // Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void GetLineText_LineSpanningMultiplePieces_ReturnsCorrectText()
    {
        // Arrange
        var pieceTable = new PieceTable("Hello");
        pieceTable.Insert(5, "\nWorld");

        // Act
        var result = pieceTable.GetLineText(1);

        // Assert
        result.ShouldBe("World");
    }
}

