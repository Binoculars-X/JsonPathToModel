using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JsonPathToModel.Exceptions;
using JsonPathToModel.Parser;

namespace JsonPathToModel.Tests.Parser;

public class TokenizerTests
{
    [Fact]
    public void Tokenizer_Should_ParseStraightDotExpression()
    {
        var tokens = Parse("$.Person.Name");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(Token.Dollar, tokens[0].Token);
        Assert.Equal(Token.Field, tokens[1].Token);
        Assert.Equal(Token.Field, tokens[2].Token);
        Assert.Equal("Person", tokens[1].Field);
        Assert.Equal("Name", tokens[2].Field);
    }

    [Fact]
    public void Tokenizer_Should_ParseStraightDot_CollectionIndexExpression()
    {
        var tokens = Parse("$.Person[0].Name");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(Token.Dollar, tokens[0].Token);
        Assert.Equal(Token.Field, tokens[1].Token);
        Assert.Equal(Token.Field, tokens[2].Token);
        Assert.Equal("Person", tokens[1].Field);
        Assert.Equal(0, tokens[1]?.CollectionDetails?.Index);
        Assert.Equal("", tokens[1]?.CollectionDetails?.Literal);
        Assert.Equal("Name", tokens[2].Field);
    }

    [Fact]
    public void Tokenizer_Should_ParseStraightDot_CollectionStringExpression()
    {
        var tokens = Parse("$.Customer.Person['xyz'].Name.Value");
        Assert.Equal(5, tokens.Count);
        Assert.Equal(Token.Dollar, tokens[0].Token);
        Assert.Equal(Token.Field, tokens[1].Token);
        Assert.Equal(Token.Field, tokens[2].Token);
        Assert.Equal(Token.Field, tokens[3].Token);
        Assert.Equal(Token.Field, tokens[4].Token);
        Assert.Equal("Person", tokens[2].Field);
        Assert.Equal("xyz", tokens[2]?.CollectionDetails?.Literal);
        Assert.Equal("Name", tokens[3].Field);
        Assert.Equal("Value", tokens[4].Field);
    }

    [Fact]
    public void Tokenizer_Should_ParseStraightDot_CollectionAll()
    {
        var tokens = Parse("$.Person[].Name");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(Token.Dollar, tokens[0].Token);
        Assert.Equal(Token.Field, tokens[1].Token);
        Assert.Equal(Token.Field, tokens[2].Token);
        Assert.Equal("Person", tokens[1].Field);
        Assert.Null(tokens[1]?.CollectionDetails?.Index);
        Assert.Equal("", tokens[1]?.CollectionDetails?.Literal);
        Assert.True(tokens[1]?.CollectionDetails?.SelectAll);
        Assert.Equal("Name", tokens[2].Field);
    }

    [Fact]
    public void Tokenizer_Should_ParseStraightDot_CollectionAsteriskAll()
    {
        var tokens = Parse("$.Person[*].Name");
        Assert.Equal(3, tokens.Count);
        Assert.Equal(Token.Dollar, tokens[0].Token);
        Assert.Equal(Token.Field, tokens[1].Token);
        Assert.Equal(Token.Field, tokens[2].Token);
        Assert.Equal("Person", tokens[1].Field);
        Assert.Null(tokens[1]?.CollectionDetails?.Index);
        Assert.Equal("", tokens[1]?.CollectionDetails?.Literal);
        Assert.True(tokens[1]?.CollectionDetails?.SelectAll);
        Assert.Equal("Name", tokens[2].Field);
    }

    [Fact]
    public void Tokenizer_Should_Error_WhenCollectionStringExpressionWrong()
    {
        Assert.Throws<ParserException>(() => Parse("$.Customer.Person['xyz'*].Name.Value"));
        Assert.Throws<ParserException>(() => Parse("$.Customer.Person[**].Name.Value"));
        Assert.Throws<ParserException>(() => Parse("$.Customer.Person[xyz].Name.Value"));
        Assert.Throws<ParserException>(() => Parse("$.Customer.Person['xyz].Name.Value"));
        Assert.Throws<ParserException>(() => Parse("$.Customer.Person[xyz'].Name.Value"));
        Assert.Throws<ParserException>(() => Parse("$.Customer.Person[1.0].Name.Value"));
        Assert.Throws<ParserException>(() => Parse("$.Customer.Person['''].Name.Value"));
    }

    [Fact]
    public void Tokenizer_Should_Error_WhenCrazyWrongExpression()
    {
        Assert.Throws<ParserException>(() => Parse("$.['Customer'].Value"));
        Assert.Throws<ParserException>(() => Parse("Customer.Person['xyz].Name.Value"));
        Assert.Throws<ParserException>(() => Parse("$...Name.Value"));
        Assert.Throws<ParserException>(() => Parse("$. Customer . Person .Name.*"));
        Assert.Throws<ParserException>(() => Parse("$.*"));
        Assert.Throws<ParserException>(() => Parse("$.Cust omer.Person.Name.Value"));
    }

    private List<TokenInfo> Parse(string text)
    {
        var tokenList = new List<TokenInfo>();

        using (TextReader sr = new StringReader(text))
        {
            var t = new Tokenizer(sr);

            while (t.Token != Token.EOF)
            {
                tokenList.Add(t.Info);
                t.NextToken();
            }
        }

        return tokenList;
    }
}
