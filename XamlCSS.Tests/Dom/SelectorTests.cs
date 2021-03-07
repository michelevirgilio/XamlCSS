﻿using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using XamlCSS.CssParsing;
using XamlCSS.Dom;

namespace XamlCSS.Tests.Dom
{
    [TestFixture]
    public class SelectorTests
    {
        private StyleSheet defaultStyleSheet;

        public SelectorTests()
        {

        }

        [SetUp]
        public void Setup()
        {
            CssParser.defaultCssNamespace = "XamlCSS.Tests.Dom, XamlCSS.Tests, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null";
            defaultStyleSheet = CssParser.Parse(@"@namespace ui ""XamlCSS.Tests.Dom, XamlCSS.Tests, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null"";");
        }

        [Test]
        public void Test()
        {
            var selector = new Selector(".button a> #bbb");
            
            selector.selectorMatchers.Select(x => x.Text).Should().BeEquivalentTo(new[] { "button", " ", "a", ">", "bbb" }.ToList());
            selector.selectorMatchers.Select(x => x.Type).Should().BeEquivalentTo(new[] {
                CssNodeType.ClassSelector,
                CssNodeType.GeneralDescendantCombinator,
                CssNodeType.TypeSelector,
                CssNodeType.DirectDescendantCombinator,
                CssNodeType.IdSelector
            }.ToList());
        }

        [Test]
        public void Can_match_id()
        {
            var selector = new Selector("#bbb");

            var tag = GetDomElement("button", "bbb");
            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_tagname()
        {
            var selector = new Selector("button");

            var tag = GetDomElement("button", "bbb");
            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_class()
        {
            var selector = new Selector(".important");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_class_multiple()
        {
            var selector = new Selector(".important.some");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_tagname_with_class()
        {
            var selector = new Selector("button.important");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_id_with_class()
        {
            var selector = new Selector("#bbb.important");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_direct_sibling()
        {
            var selector = new Selector("a+#bbb");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("a", null);

            var parent = GetDomElement("parent", null, "", new[] { sibling, tag });

            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_direct_sibling_more_levels()
        {
            var selector = new Selector("a+b+#bbb");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("a", null);
            var sibling2 = GetDomElement("b", null);

            var parent = GetDomElement("parent", null, "", new[] { sibling, sibling2, tag });

            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_general_sibling()
        {
            var selector = new Selector("a~#bbb");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("a", null);
            var sibling2 = GetDomElement("b", null);

            var parent = GetDomElement("parent", null, "", new[] { sibling, sibling2, tag });

            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_general_sibling_more_levels()
        {
            var selector = new Selector("c~d a~#bbb");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("a", null);
            var sibling2 = GetDomElement("b", null);

            var parent = GetDomElement("d", null, "", new[] { sibling, sibling2, tag });
            var parentSibling = GetDomElement("c", null, "", null);
            var parentSibling2 = GetDomElement("y", null, "", null);

            var parentRoot = GetDomElement("parent", null, "", new[] { parentSibling, parentSibling2, parent });

            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_direct_descendant()
        {
            var selector = new Selector("a>#bbb");

            var tag = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { tag });

            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_direct_descendant_more_levels()
        {
            var selector = new Selector("b>a>#bbb");

            var tag = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { tag });
            var parentRoot = GetDomElement("b", null, "", new[] { parent });

            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_general_descendant()
        {
            var selector = new Selector("a #bbb");

            var tag = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { tag });
            var parentRoot = GetDomElement("a", null, "", new[] { parent });

            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_general_descendant_more_levels()
        {
            var selector = new Selector("b a #bbb");

            var tag = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { tag });
            var parentRoot = GetDomElement("b", null, "", new[] { parent });

            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_first_child()
        {
            var selector = new Selector(":first-child");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { tag, sibling });

            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_last_child()
        {
            var selector = new Selector(":last-child");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");
            var sibling2 = GetDomElement("x", "bbb", "some important stuff");
            var sibling3 = GetDomElement("button", "bbb", "some important stuff");
            var sibling4 = GetDomElement("y", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { sibling, sibling2, sibling3, sibling4, tag });

            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_nth_last_child_1()
        {
            var selector = new Selector(":nth-last-child(1)");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");
            var sibling2 = GetDomElement("x", "bbb", "some important stuff");
            var sibling3 = GetDomElement("button", "bbb", "some important stuff");
            var sibling4 = GetDomElement("y", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { sibling, sibling2, sibling3, sibling4, tag });

            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_nth_last_child_2n_1()
        {
            var selector = new Selector(":nth-last-child(2n+1)");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");
            var sibling2 = GetDomElement("x", "bbb", "some important stuff");
            var sibling3 = GetDomElement("button", "bbb", "some important stuff");
            var sibling4 = GetDomElement("y", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { sibling, sibling2, sibling3, tag });

            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_nth_last_child_2n_minus_1()
        {
            var selector = new Selector(":nth-last-child(2n-1)");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { sibling, tag });

            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_nth_last_child_3n_minus_1()
        {
            var selector = new Selector(":nth-child(3n-1)");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");
            var sibling2 = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { sibling, tag, sibling2 });

            selector.Match(defaultStyleSheet, sibling).IsSuccess.Should().Be(false);
            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling2).IsSuccess.Should().Be(false);
        }

        [Test]
        public void Can_match_nth_child_2()
        {
            var selector = new Selector(":nth-child(2)");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { sibling, tag });

            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_nth_child_2n()
        {
            var selector = new Selector(":nth-child(2n)");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");
            var sibling3 = GetDomElement("button", "bbb", "some important stuff");
            var sibling4 = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { sibling, tag, sibling3, sibling4 });

            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling4).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_nth_child_2n_plus_1()
        {
            var selector = new Selector(":nth-child(2n+1)");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");
            var sibling3 = GetDomElement("button", "bbb", "some important stuff");
            var sibling4 = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { tag, sibling, sibling3, sibling4 });

            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling3).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_nth_child_2n_minus_1()
        {
            var selector = new Selector(":nth-child(2n-1)");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");
            var sibling3 = GetDomElement("button", "bbb", "some important stuff");
            var sibling4 = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { tag, sibling, sibling3, sibling4 });

            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling3).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_nth_child_minus_n_plus_2()
        {
            var selector = new Selector(":nth-child(-n+2)");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");
            var sibling3 = GetDomElement("button", "bbb", "some important stuff");
            var sibling4 = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { tag, sibling, sibling3, sibling4 });

            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling3).IsSuccess.Should().Be(false);
            selector.Match(defaultStyleSheet, sibling4).IsSuccess.Should().Be(false);
        }

        [Test]
        public void Can_match_nth_child_minus_2n_plus_3()
        {
            var selector = new Selector(":nth-child(-2n+3)");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");
            var sibling3 = GetDomElement("button", "bbb", "some important stuff");
            var sibling4 = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { tag, sibling, sibling3, sibling4 });

            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling).IsSuccess.Should().Be(false);
            selector.Match(defaultStyleSheet, sibling3).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling4).IsSuccess.Should().Be(false);
        }

        [Test]
        public void Can_match_nth_of_type()
        {
            var selector = new Selector(":nth-of-type(2)");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");
            var sibling2 = GetDomElement("x", "bbb", "some important stuff");
            var sibling3 = GetDomElement("button", "bbb", "some important stuff");
            var sibling4 = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { sibling, sibling2, tag, sibling3, sibling4 });

            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling4).IsSuccess.Should().Be(false);
        }

        [Test]
        public void Can_match_nth_of_type_2n_plus_2()
        {
            var selector = new Selector(":nth-of-type(2n+2)");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");
            var sibling2 = GetDomElement("x", "bbb", "some important stuff");
            var sibling3 = GetDomElement("button", "bbb", "some important stuff");
            var sibling4 = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { sibling, sibling2, tag, sibling3, sibling4 });

            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling4).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_nth_of_type_n_plus_1()
        {
            var selector = new Selector(":nth-of-type(n+1)");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");
            var sibling2 = GetDomElement("x", "bbb", "some important stuff");
            var sibling3 = GetDomElement("button", "bbb", "some important stuff");
            var sibling4 = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { sibling, sibling2, tag, sibling3, sibling4 });

            selector.Match(defaultStyleSheet, sibling).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling3).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling4).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_nth_of_type_n_plus_2()
        {
            var selector = new Selector(":nth-of-type(n+2)");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");
            var sibling2 = GetDomElement("x", "bbb", "some important stuff");
            var sibling3 = GetDomElement("button", "bbb", "some important stuff");
            var sibling4 = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { sibling, sibling2, tag, sibling3, sibling4 });

            selector.Match(defaultStyleSheet, sibling).IsSuccess.Should().Be(false);
            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling3).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling4).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_nth_last_of_type()
        {
            var selector = new Selector(":nth-last-of-type(2)");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");
            var sibling2 = GetDomElement("x", "bbb", "some important stuff");
            var sibling3 = GetDomElement("button", "bbb", "some important stuff");
            var sibling4 = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { sibling, sibling2, tag, sibling3, sibling4 });

            selector.Match(defaultStyleSheet, sibling).IsSuccess.Should().Be(false);
            selector.Match(defaultStyleSheet, sibling2).IsSuccess.Should().Be(false);
            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(false);
            selector.Match(defaultStyleSheet, sibling3).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling4).IsSuccess.Should().Be(false);
        }

        [Test]
        public void Can_match_nth_last_of_type_2n_plus_2()
        {
            var selector = new Selector(":nth-last-of-type(2n+2)");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");
            var sibling2 = GetDomElement("x", "bbb", "some important stuff");
            var sibling3 = GetDomElement("button", "bbb", "some important stuff");
            var sibling4 = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { sibling, sibling2, tag, sibling3, sibling4 });

            selector.Match(defaultStyleSheet, sibling).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(false);
            selector.Match(defaultStyleSheet, sibling3).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling4).IsSuccess.Should().Be(false);
        }

        [Test]
        public void Can_match_nth_last_of_type_n_plus_1()
        {
            var selector = new Selector(":nth-last-of-type(n+1)");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");
            var sibling2 = GetDomElement("x", "bbb", "some important stuff");
            var sibling3 = GetDomElement("button", "bbb", "some important stuff");
            var sibling4 = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { sibling, sibling2, tag, sibling3, sibling4 });

            selector.Match(defaultStyleSheet, sibling).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling2).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling3).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling4).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_nth_last_of_type_n_plus_2()
        {
            var selector = new Selector(":nth-last-of-type(n+2)");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");
            var sibling2 = GetDomElement("x", "bbb", "some important stuff");
            var sibling3 = GetDomElement("button", "bbb", "some important stuff");
            var sibling4 = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { sibling, sibling2, tag, sibling3, sibling4 });

            selector.Match(defaultStyleSheet, sibling).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling2).IsSuccess.Should().Be(false);
            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling3).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling4).IsSuccess.Should().Be(false);
        }




        [Test]
        public void Can_match_first_of_type()
        {
            var selector = new Selector(":first-of-type");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");
            var sibling2 = GetDomElement("x", "bbb", "some important stuff");
            var sibling3 = GetDomElement("button", "bbb", "some important stuff");
            var sibling4 = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { sibling, sibling2, tag, sibling3, sibling4 });

            selector.Match(defaultStyleSheet, sibling).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling2).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(false);
            selector.Match(defaultStyleSheet, sibling3).IsSuccess.Should().Be(false);
            selector.Match(defaultStyleSheet, sibling4).IsSuccess.Should().Be(false);
        }

        [Test]
        public void Can_match_last_of_type()
        {
            var selector = new Selector(":last-of-type");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");
            var sibling2 = GetDomElement("x", "bbb", "some important stuff");
            var sibling3 = GetDomElement("button", "bbb", "some important stuff");
            var sibling4 = GetDomElement("button", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { sibling, sibling2, tag, sibling3, sibling4 });

            selector.Match(defaultStyleSheet, sibling).IsSuccess.Should().Be(false);
            selector.Match(defaultStyleSheet, sibling2).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(false);
            selector.Match(defaultStyleSheet, sibling3).IsSuccess.Should().Be(false);
            selector.Match(defaultStyleSheet, sibling4).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_only_of_type()
        {
            var selector = new Selector(":only-of-type");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");
            var sibling2 = GetDomElement("x", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { sibling, sibling2, tag });

            selector.Match(defaultStyleSheet, sibling).IsSuccess.Should().Be(false);
            selector.Match(defaultStyleSheet, sibling2).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(false);
        }

        [Test]
        public void Can_match_universal()
        {
            var selector = new Selector("*");

            var tag = GetDomElement("button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");
            var sibling2 = GetDomElement("x", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { sibling, sibling2, tag });

            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, parent).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling2).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_universal_namespaced()
        {
            var selector = new Selector("ui|*");

            var tag = GetDomElement("ui|button", "bbb", "some important stuff");
            var sibling = GetDomElement("button", "bbb", "some important stuff");
            var sibling2 = GetDomElement("x", "bbb", "some important stuff");

            var parent = GetDomElement("a", null, "", new[] { sibling, sibling2, tag });

            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, parent).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling2).IsSuccess.Should().Be(true);
        }

        [Test]
        public void Can_match_derived_type()
        {
            defaultStyleSheet = CssParser.Parse(@"@namespace ui ""XamlCSS.Tests.Dom, XamlCSS.Tests, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null"";");
            var selector = new Selector("^ui|UIElement");

            var tag = GetDomElement("ui|UIElement", "bbb", "some important stuff");
            var sibling = GetDomElement("ui|UIElement", "bbb", "some important stuff");
            var sibling2 = GetDomElement("ui|DerivedUIElement", "bbb", "some important stuff", uiElement: new DerivedUIElement());

            selector.Match(defaultStyleSheet, tag).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling).IsSuccess.Should().Be(true);
            selector.Match(defaultStyleSheet, sibling2).IsSuccess.Should().Be(true);
        }

        public TestNode GetDomElement(string tagname, string id, string classes = "", IEnumerable<IDomElement<UIElement, PropertyInfo>> children = null, UIElement uiElement = null)
        {
            var node = new TestNode(uiElement ?? GetUiElement(), null, tagname, children, null, id, classes);

            node.StyleInfo.CurrentStyleSheet = defaultStyleSheet;

            return node;
        }

        public UIElement GetUiElement()
        {
            return new UIElement();
        }
    }
}
