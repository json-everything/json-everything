using System;
using NUnit.Framework;

namespace Json.Pointer.Tests;

public class JsonPointerSegmentTests
{
    [Test]
    public void SegmentEqualsString()
    {
        var pointer = JsonPointer.Parse("/foo/bar");
        var segment = pointer[1];

        Assert.That(segment == "bar", Is.True);
        Assert.That(segment == "baz", Is.False);
        Assert.That(segment != "baz", Is.True);
        Assert.That(segment != "bar", Is.False);
    }

    [Test]
    public void SegmentEqualsSpan()
    {
        var pointer = JsonPointer.Parse("/foo/bar");
        var segment = pointer[1];
        var span = "bar".AsSpan();

        Assert.That(segment == span, Is.True);
        Assert.That(segment == "baz".AsSpan(), Is.False);
        Assert.That(segment != "baz".AsSpan(), Is.True);
        Assert.That(segment != span, Is.False);
    }

    [Test]
    public void SegmentEqualsSegment()
    {
        var pointer1 = JsonPointer.Parse("/foo/bar");
        var pointer2 = JsonPointer.Parse("/baz/bar");
        var segment1 = pointer1[1];
        var segment2 = pointer2[1];

        Assert.That(segment1 == segment2, Is.True);
        Assert.That(segment1 == pointer1[0], Is.False);
        Assert.That(segment1 != pointer1[0], Is.True);
        Assert.That(segment1 != segment2, Is.False);
    }

    [Test]
    public void SegmentToString()
    {
        var pointer = JsonPointer.Parse("/foo/bar");
        var segment = pointer[1];

        Assert.That(segment.ToString(), Is.EqualTo("bar"));
    }

    [Test]
    public void SegmentAsSpan()
    {
        var pointer = JsonPointer.Parse("/foo/bar");
        var segment = pointer[1];
        var span = segment.AsSpan();

        Assert.That(span.SequenceEqual("bar".AsSpan()), Is.True);
    }

    [Test]
    public void PointerIndexer()
    {
        var pointer = JsonPointer.Parse("/foo/bar");

        Assert.That(pointer[0] == "foo", Is.True);
        Assert.That(pointer[1] == "bar", Is.True);
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = pointer[2]);
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = pointer[-1]);
    }

    [Test]
    public void PointerTryGetSegment()
    {
        var pointer = JsonPointer.Parse("/foo/bar");

        Assert.That(pointer.TryGetSegment(0, out var segment0), Is.True);
        Assert.That(segment0 == "foo", Is.True);

        Assert.That(pointer.TryGetSegment(1, out var segment1), Is.True);
        Assert.That(segment1 == "bar", Is.True);

        Assert.That(pointer.TryGetSegment(2, out _), Is.False);
        Assert.That(pointer.TryGetSegment(-1, out _), Is.False);
    }

    [Test]
    public void SegmentEqualsWithEncodedValues()
    {
        var pointer = JsonPointer.Parse("/foo~0bar/foo~1bar/foo~0~1bar");
        
        // Test ~0 (tilde)
        Assert.That(pointer[0] == "foo~bar", Is.True);
        Assert.That(pointer[0] == "foo~0bar", Is.False);
        
        // Test ~1 (forward slash)
        Assert.That(pointer[1] == "foo/bar", Is.True);
        Assert.That(pointer[1] == "foo~1bar", Is.False);
        
        // Test ~0~1 (tilde followed by forward slash)
        Assert.That(pointer[2] == "foo~/bar", Is.True);
        Assert.That(pointer[2] == "foo~0~1bar", Is.False);
    }

    [Test]
    public void SegmentEqualsWithUnencodedValues()
    {
        var pointer = JsonPointer.Parse("/foo~0bar/foo~1bar/foo~0~1bar");
        
        // Test unencoded tilde
        Assert.That(pointer[0] == "foo~bar", Is.True);
        Assert.That(pointer[0] == "foo~0bar", Is.False);
        
        // Test unencoded forward slash
        Assert.That(pointer[1] == "foo/bar", Is.True);
        Assert.That(pointer[1] == "foo~1bar", Is.False);
        
        // Test unencoded tilde followed by forward slash
        Assert.That(pointer[2] == "foo~/bar", Is.True);
        Assert.That(pointer[2] == "foo~0~1bar", Is.False);
    }

    [Test]
    public void SegmentEqualsWithMixedEncodedAndUnencodedValues()
    {
        var pointer = JsonPointer.Parse("/foo~0bar/foo~1bar/foo~0~1bar");
        
        // Test encoded tilde against unencoded
        Assert.That(pointer[0] == "foo~bar", Is.True);
        Assert.That(pointer[0] == "foo~0bar", Is.False);
        
        // Test unencoded forward slash against encoded
        Assert.That(pointer[1] == "foo/bar", Is.True);
        Assert.That(pointer[1] == "foo~1bar", Is.False);
        
        // Test encoded tilde+slash against unencoded
        Assert.That(pointer[2] == "foo~/bar", Is.True);
        Assert.That(pointer[2] == "foo~0~1bar", Is.False);
    }

    [Test]
    public void SegmentEqualsWithEmptyValues()
    {
        var pointer = JsonPointer.Parse("//");
        
        Assert.That(pointer[0] == "", Is.True);
        Assert.That(pointer[1] == "", Is.True);
    }

    [Test]
    public void SegmentEqualsWithSpecialCharacters()
    {
        var pointer = JsonPointer.Parse("/!@#$%^&*()_+-=[]{}|;:,.<>?");
        
        Assert.That(pointer[0] == "!@#$%^&*()_+-=[]{}|;:,.<>?", Is.True);
    }
} 