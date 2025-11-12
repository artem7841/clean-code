using System.Collections.Generic;
using System.Text;

namespace Markdown
{
    public class Parser
    {
        private List<Token> tokens;
        private int position;

        public MainNode ParseTokensToTree(List<Token> tokenList)
        {
            tokens = tokenList;
            position = 0;

            var root = new MainNode();

            while (position < tokens.Count)
            {
                var node = ParseToken(); 
                if (node != null)
                    root.Children.Add(node);
            }

            return root;
        }

        private Node ParseToken(bool inEmphasisContext = false)
        {
            if (position >= tokens.Count)
                return null;

            var token = tokens[position];

            switch (token.Type)
            {
                case TokenType.Text:
                case TokenType.Space:
                    position++;
                    return new TextNode(token.Value);

                case TokenType.NextLine:
                    position++;
                    return new NextLineNode();

                case TokenType.Header:
                    return ParseHeader();

                case TokenType.Emphasis:
                    return ParseEmphasisNode(inEmphasisContext);

                case TokenType.Strong:
                    return inEmphasisContext 
                        ? ParseStrongAsText(token) 
                        : ParseStrongNode(inEmphasisContext);

                default:
                    position++;
                    return new TextNode(token.Value);
            }
        }

        private Node ParseHeader()
        {
            var start = position;
            position++; 

            if (position >= tokens.Count || 
                tokens[position].Type != TokenType.Space || 
                tokens[position].Value != " ")  
            {
                return new TextNode("#");
            }

            position++; 

            var header = new HeaderNode();
            while (position < tokens.Count && tokens[position].Type != TokenType.NextLine)
                header.Children.Add(ParseToken());

            return header;
        }



        private bool TrySkipListItemPrefix()
        {
            if (position >= tokens.Count || tokens[position].Type != TokenType.ListItem)
                return false;
        
            position++; 
    
            if (position >= tokens.Count || tokens[position].Type != TokenType.Space)  
                return false;
        
            position++; 
            return true;
        }

        

        private Node ParseEmphasisNode(bool inEmphasisContext = false)
        {
            var startPos = position;
            position++; 
            
            if (!IsValidEmphasisOpening(startPos))
                return new TextNode("_");
            
            var isMidWord = IsMidWordEmphasis(startPos);
            var node = new EmphasisNode();
            var hasSpaceInside = false;

            ProcessEmphasisContent(node, ref hasSpaceInside, inEmphasisContext);
            
            var hasClosing = position < tokens.Count && 
                             tokens[position].Type == TokenType.Emphasis;
            
            if (ShouldCreateEmphasisNode(hasClosing, isMidWord, hasSpaceInside))
            {
                position++;
                return node;
            }
            
            return CreateTextNodeFallback(startPos);
        }

        private bool IsValidEmphasisOpening(int startPos)
        {
            if (startPos + 1 >= tokens.Count) 
                return false;

            var nextToken = tokens[startPos + 1];
            return IsValidTokenBoundary(nextToken);
        }

        private bool IsMidWordEmphasis(int startPos)
        {
            if (startPos == 0) 
                return false;

            var previousToken = tokens[startPos - 1];
            return previousToken.Type == TokenType.Text;
        }

        private void ProcessEmphasisContent(EmphasisNode node, ref bool hasSpaceInside, bool inEmphasisContext)
        {
            while (position < tokens.Count && tokens[position].Type != TokenType.Emphasis)
            {
                var currentToken = tokens[position];
                
                if (currentToken.Type == TokenType.Space || currentToken.Type == TokenType.NextLine)
                    hasSpaceInside = true;

                switch (currentToken.Type)
                {
                    case TokenType.Emphasis:
                        node.Children.Add(ParseEmphasisNode(inEmphasisContext: true));
                        break;
                    case TokenType.Strong:
                        node.Children.Add(new TextNode(""));
                        position++; 
                        break;
                    default:
                        node.Children.Add(ParseToken(inEmphasisContext: true));
                        break;
                }
            }
        }

        private bool ShouldCreateEmphasisNode(bool hasClosing, bool isMidWord, bool hasSpaceInside)
        {
            if (!hasClosing)
                return false;

            if (!IsValidEmphasisClosing())
                return false;
            
            return !isMidWord || !hasSpaceInside;
        }

        private bool IsValidEmphasisClosing()
        {
            if (position == 0) 
                return false;

            var previousToken = tokens[position - 1];
            return IsValidTokenBoundary(previousToken);
        }

        private Node ParseStrongNode(bool inEmphasisContext)
        {
            var startPos = position;
    
            if (position >= tokens.Count || tokens[position].Type != TokenType.Strong)
                return new TextNode(tokens[position++].Value);

            if (!IsValidStrongOpening())
                return new TextNode(tokens[position++].Value);

            position++; 

            var node = new StrongNode();
            ProcessStrongContent(node, inEmphasisContext);

            var hasClosing = position < tokens.Count && tokens[position].Type == TokenType.Strong;

            if (hasClosing && IsValidStrongClosing())
            {
                position++; 
                return node;
            }
            
            return CreateTextNodeFallback(startPos);
        }

        private Node ParseStrongAsText(Token token)
        {
            position++;
            return new TextNode(token.Value);
        }

        private bool IsValidStrongOpening()
        {
            if (position + 1 >= tokens.Count) 
                return false;

            var nextToken = tokens[position + 1];
            return IsValidTokenBoundary(nextToken);
        }

        private bool IsValidStrongClosing()
        {
            if (position == 0) 
                return false;

            var previousToken = tokens[position - 1];
            return IsValidTokenBoundary(previousToken);
        }

        private bool IsValidTokenBoundary(Token token)
        {
            return token.Type != TokenType.Space && 
                   token.Type != TokenType.NextLine &&
                   !(token.Type == TokenType.Text && token.Value.Length > 0 && char.IsDigit(token.Value[0]));
        }

        private void ProcessStrongContent(StrongNode node, bool inEmphasisContext)
        {
            while (position < tokens.Count && tokens[position].Type != TokenType.Strong)
            {
                if (tokens[position].Type == TokenType.Emphasis)
                {
                    node.Children.Add(ParseEmphasisNode(inEmphasisContext: inEmphasisContext));
                }
                else
                {
                    node.Children.Add(ParseToken(inEmphasisContext: inEmphasisContext));
                }
            }
        }

        private Node CreateTextNodeFallback(int startPos)
        {
            var text = new StringBuilder();
            for (int i = startPos; i < position; i++)
                text.Append(tokens[i].Value);
            return new TextNode(text.ToString());
        }
    }
}