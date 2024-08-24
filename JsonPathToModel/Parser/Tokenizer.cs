using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using static JsonPathToModel.Parser.Tokenizer;

namespace JsonPathToModel.Parser;

public class Tokenizer
{
    //private readonly IObjectResolver _resolver;
    //private readonly HashSet<string> _operators;
    private const char EOF = '\0';


    public Tokenizer(TextReader reader)
    {
        _reader = reader;
        NextChar();
        NextToken();
    }

    TextReader _reader;
    char _currentChar;
    Token _currentToken;
    bool _isPreviousDot;

    public string Field { get; private set; }
    public CollectionDef? Collection { get; private set; }
    //public string Param { get; private set; }
    //public decimal Number { get; private set; }
    //public string StringLiteral { get; private set; }
    //public string Operator { get; private set; }

    public Token Token
    {
        get { return _currentToken; }
    }

    public record CollectionDef(int? Index, string? Literal)
    {
        public bool SelectAll { get { return Index == null && string.IsNullOrEmpty(Literal); } }
    }

    public TokenInfo Info
    {
        get
        {
            return new TokenInfo
            {
                Token = Token,
                Field = Field,
                Collection = Collection,
                //Number = Number,
                //Operator = Operator,
                //Param = Param,
                //StringLiteral = StringLiteral
            };
        }
    }

    // Read the next character from the input strem
    // and store it in _currentChar, or load '\0' if EOF
    void NextChar()
    {
        CleanTokenProps();
        int ch = _reader.Read();
        _currentChar = ch < 0 ? '\0' : (char)ch;
    }

    private void CleanTokenProps()
    {
        Field = null; 
        Collection = null; 
        //Number = 0; 
        //Operator = null; 
        //Param = null; 
        //StringLiteral = null;
    }

    // Read the next token from the input stream
    public void NextToken()
    {
        // throw whitespace
        while (char.IsWhiteSpace(_currentChar))
        {
            throw new ParserException($"Illigal unexpected spaces");
            //NextChar();
        }

        // Special characters
        switch (_currentChar)
        {
            case EOF:
                _currentToken = Token.EOF;
                return;

            case '$':
                _currentToken = Token.Dollar;
                _isPreviousDot = false;
                NextChar();
                return;

        }

        // Dot
        if (_currentChar == '.')
        {
            if (_isPreviousDot)
            {
                throw new ParserException($"Illigal characters '..', a field is expected");
            }
            
            _isPreviousDot = true;
            NextChar();
        }

        // Field - starts with letter or underscore: _field Field field[] field[*] field[0] field['xyz']
        // also may be operator
        var fieldName = ReadFieldName();

        if (fieldName != "")
        {
            var collectionDefinition = ReadCollectionDefinition();

            Field = fieldName;
            Collection = collectionDefinition;
            _currentToken = Token.Field;
            _isPreviousDot = false;
            return;
        }

        throw new ParserException($"Illigal symbol '{_currentChar}'");
    }

    private string ReadFieldName()
    {
        if (char.IsLetter(_currentChar) || _currentChar == '_')
        {
            var sb = new StringBuilder();
            bool isCollection;

            // Accept letter, digit or underscore
            while (char.IsLetterOrDigit(_currentChar) || _currentChar == '_')
            {
                sb.Append(_currentChar);
                NextChar();
            }

            var field = sb.ToString();

            return field;
        }

        return "";
    }

    private string ReadStringLiteral()
    {
        if (_currentChar == '\'')
        {
            var sb = new StringBuilder();
            NextChar();

            while (_currentChar != '\'')
            {
                if (_currentChar == EOF)
                {
                    throw new ParserException($"Symbol ' is expected before EOF");
                }

                sb.Append(_currentChar);
                NextChar();
            }

            NextChar();
            var stringLiteral = sb.ToString();
            return stringLiteral;
        }

        return "";
    }

    private int? ReadIntNumber()
    {
        if (char.IsDigit(_currentChar))
        {
            // Capture int
            var sb = new StringBuilder();
            while (char.IsDigit(_currentChar))
            {
                sb.Append(_currentChar);
                NextChar();
            }

            // Parse it
            var number = int.Parse(sb.ToString(), CultureInfo.InvariantCulture);
            return number;
        }

        return null;
    }

    // ToDo: finish
    private CollectionDef? ReadCollectionDefinition()
    {
        bool isCollection = false;
        int? index = null;
        string? literal = null;

        if (_currentChar == '[')
        {
            isCollection = true;
            NextChar();
        }

        index = ReadIntNumber();
        literal = ReadStringLiteral();

        // Asterisk means all elements
        if (_currentChar == '*')
        {
            if (literal != null && literal != "")
            {
                throw new ParserException($"Illigal symbol '{_currentChar}' used with string literal, ']' is expected");
            }

            literal = "";
            NextChar();
        }

        if (_currentChar == ']')
        {
            NextChar();
        }
        else if (isCollection)
        {
            throw new ParserException($"Illigal symbol '{_currentChar}', ']' is expected");
        }

        if (isCollection)
        { 
            return new CollectionDef(index, literal);
        }

        return null;
    }
}




//public interface IObjectResolver
//{
//    string[] GetQueryFields();
//    string[] GetQueryParams();
//    string[] GetOperators();
//    Dictionary<int, string[]> GetPriorityOperators();
//}