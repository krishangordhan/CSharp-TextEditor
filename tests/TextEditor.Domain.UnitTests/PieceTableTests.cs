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
}

