using XmppDotNet.Attributes;

namespace XmppDotNet.Xmpp.XHtmlIM
{
    [XmppTag(Name = "head", Namespace = Namespaces.Xhtml)]
    public class Head : XHtmlIMElement
    {
        public Head()
            : base("head")
        {
        }
    }

    [XmppTag(Name = "title", Namespace = Namespaces.Xhtml)]
    public class Title : XHtmlIMElement
    {
        public Title()
            : base("title")
        {
        }
    }

    #region << text module elements >>
    [XmppTag(Name = "blockquote", Namespace = Namespaces.Xhtml)]
    public class Blockquote : XHtmlIMElement
    {
        public Blockquote()
            : base("blockquote")
        {
        }
    }

    [XmppTag(Name = "br", Namespace = Namespaces.Xhtml)]
    public class Br : XHtmlIMElement
    {
        public Br()
            : base("br")
        {
        }
    }

    [XmppTag(Name = "cite", Namespace = Namespaces.Xhtml)]
    public class Cite : XHtmlIMElement
    {
        public Cite()
            : base("cite")
        {
        }
    }

    [XmppTag(Name = "em", Namespace = Namespaces.Xhtml)]
    public class Em : XHtmlIMElement
    {
        public Em()
            : base("em")
        {
        }
    }

    [XmppTag(Name = "p", Namespace = Namespaces.Xhtml)]
    public class P : XHtmlIMElement
    {
        public P()
            : base("p")
        {
        }
    }

    [XmppTag(Name = "span", Namespace = Namespaces.Xhtml)]
    public class Span : XHtmlIMElement
    {
        public Span()
            : base("span")
        {
        }
    }

    [XmppTag(Name = "strong", Namespace = Namespaces.Xhtml)]
    public class Strong : XHtmlIMElement
    {
        public Strong()
            : base("strong")
        {
        }
    }
    #endregion

    [XmppTag(Name = "a", Namespace = Namespaces.Xhtml)]
    public class A : XHtmlIMElement
    {
        public A()
            : base("a")
        {
        }
    }

    #region << headers >>
   [XmppTag(Name = "h1", Namespace = Namespaces.Xhtml)]
    public class H1 : XHtmlIMElement
    {
        public H1()
            : base("h1")
        {
        }
    }

    [XmppTag(Name = "h2", Namespace = Namespaces.Xhtml)]
    public class H2 : XHtmlIMElement
    {
        public H2()
            : base("h2")
        {
        }
    }

    [XmppTag(Name = "h3", Namespace = Namespaces.Xhtml)]
    public class H3 : XHtmlIMElement
    {
        public H3()
            : base("h3")
        {
        }
    }

    [XmppTag(Name = "h4", Namespace = Namespaces.Xhtml)]
    public class H4 : XHtmlIMElement
    {
        public H4()
            : base("h4")
        {
        }
    }

    [XmppTag(Name = "h5", Namespace = Namespaces.Xhtml)]
    public class H5 : XHtmlIMElement
    {
        public H5()
            : base("h5")
        {
        }
    }

    [XmppTag(Name = "h6", Namespace = Namespaces.Xhtml)]
    public class H6 : XHtmlIMElement
    {
        public H6()
            : base("h6")
        {
        }
    }
    #endregion

    [XmppTag(Name = "img", Namespace = Namespaces.Xhtml)]
    public class Img : XHtmlIMElement
    {
        public Img()
            : base("img")
        {
        }
    }

    [XmppTag(Name = "abbr", Namespace = Namespaces.Xhtml)]
    public class Abbr : XHtmlIMElement
    {
        public Abbr()
            : base("abbr")
        {
        }
    }

    [XmppTag(Name = "acronym", Namespace = Namespaces.Xhtml)]
    public class Acronym : XHtmlIMElement
    {
        public Acronym()
            : base("acronym")
        {
        }
    }

    [XmppTag(Name = "address", Namespace = Namespaces.Xhtml)]
    public class Address : XHtmlIMElement
    {
        public Address()
            : base("address")
        {
        }
    }

    [XmppTag(Name = "code", Namespace = Namespaces.Xhtml)]
    public class Code : XHtmlIMElement
    {
        public Code()
            : base("code")
        {
        }
    }

    [XmppTag(Name = "dfn", Namespace = Namespaces.Xhtml)]
    public class Dfn : XHtmlIMElement
    {
        public Dfn()
            : base("dfn")
        {
        }
    }

    [XmppTag(Name = "div", Namespace = Namespaces.Xhtml)]
    public class Div : XHtmlIMElement
    {
        public Div()
            : base("div")
        {
        }
    }

    [XmppTag(Name = "pre", Namespace = Namespaces.Xhtml)]
    public class Pre : XHtmlIMElement
    {
        public Pre()
            : base("pre")
        {
        }
    }

    [XmppTag(Name = "q", Namespace = Namespaces.Xhtml)]
    public class Q : XHtmlIMElement
    {
        public Q()
            : base("q")
        {
        }
    }

    [XmppTag(Name = "samp", Namespace = Namespaces.Xhtml)]
    public class Samp : XHtmlIMElement
    {
        public Samp()
            : base("samp")
        {
        }
    }

    [XmppTag(Name = "var", Namespace = Namespaces.Xhtml)]
    public class Var : XHtmlIMElement
    {
        public Var()
            : base("var")
        {
        }
    }

    #region list elements
    [XmppTag(Name = "dl", Namespace = Namespaces.Xhtml)]
    public class Dl : XHtmlIMElement
    {
        public Dl()
            : base("dl")
        {
        }
    }

    [XmppTag(Name = "dt", Namespace = Namespaces.Xhtml)]
    public class Dt : XHtmlIMElement
    {
        public Dt()
            : base("dt")
        {
        }
    }

    [XmppTag(Name = "dd", Namespace = Namespaces.Xhtml)]
    public class Dd : XHtmlIMElement
    {
        public Dd()
            : base("dd")
        {
        }
    }

    [XmppTag(Name = "ol", Namespace = Namespaces.Xhtml)]
    public class Ol : XHtmlIMElement
    {
        public Ol()
            : base("ol")
        {
        }
    }

    [XmppTag(Name = "ul", Namespace = Namespaces.Xhtml)]
    public class Ul : XHtmlIMElement
    {
        public Ul()
            : base("ul")
        {
        }
    }

    [XmppTag(Name = "li", Namespace = Namespaces.Xhtml)]
    public class Li : XHtmlIMElement
    {
        public Li()
            : base("li")
        {
        }
    }
    #endregion
}
