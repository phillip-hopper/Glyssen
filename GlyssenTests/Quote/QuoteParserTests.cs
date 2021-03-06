﻿using System.Collections.Generic;
using System.Linq;
using Glyssen;
using Glyssen.Character;
using Glyssen.Quote;
using NUnit.Framework;
using SIL.WritingSystems;

namespace GlyssenTests.Quote
{
	[TestFixture]
	public class QuoteParserTests
	{
		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			// Use a test version of the file so the tests won't break every time we fix a problem in the production control file.
			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerse;
		}

		[Test]
		public void Parse_OneBlockBecomesTwo_QuoteAtEnd()
		{
			var block = new Block("p", 7, 6);
			block.BlockElements.Add(new ScriptText("He replied, «Isaiah was right when he prophesied about you.»"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He replied, ", output[0].GetText(false));
			Assert.AreEqual(7, output[0].ChapterNumber);
			Assert.AreEqual(6, output[0].InitialStartVerseNumber);
			Assert.IsTrue(output[0].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("«Isaiah was right when he prophesied about you.»", output[1].GetText(false));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual("rebuking", output[1].Delivery);
			Assert.AreEqual(7, output[1].ChapterNumber);
			Assert.AreEqual(6, output[1].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_OneBlockBecomesTwo_UnclosedQuoteAtEnd()
		{
			var block = new Block("p", 2, 10);
			block.BlockElements.Add(new ScriptText("But the angel said to them, «Do not be afraid!"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("But the angel said to them, ", output[0].GetText(false));
			Assert.AreEqual(2, output[0].ChapterNumber);
			Assert.AreEqual(10, output[0].InitialStartVerseNumber);
			Assert.IsTrue(output[0].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("«Do not be afraid!", output[1].GetText(false));
			Assert.AreEqual("angel of the LORD, an", output[1].CharacterId);
			Assert.AreEqual(2, output[1].ChapterNumber);
			Assert.AreEqual(10, output[1].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_OneBlockBecomesTwo_QuoteAtBeginning()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("«Go!» he said."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("«Go!» ", output[0].GetText(false));
			Assert.IsFalse(CharacterVerseData.IsCharacterOfType(output[0].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("he said.", output[1].GetText(false));
			Assert.IsTrue(output[1].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_OneBlockBecomesThree_TwoQuotes()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go!»  «Make me!»"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.IsTrue(output[0].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("«Go!»  ", output[1].GetText(false));
			Assert.IsFalse(CharacterVerseData.IsCharacterOfType(output[1].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("«Make me!»", output[2].GetText(false));
			Assert.IsFalse(CharacterVerseData.IsCharacterOfType(output[2].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_QuoteEndsRightBeforeNewVerseStart_QuoteCharacterIdentified()
		{
			var block = new Block("p", 15, 34);
			block.BlockElements.Add(new Verse("34"));
			block.BlockElements.Add(new ScriptText("Yecu openyogi ni, “Wutye ki mugati adi?” Gugamo ni, “Abiro, ki rec mogo matitino manok.” "));
			block.BlockElements.Add(new Verse("35"));
			block.BlockElements.Add(new ScriptText("Ociko lwak ni gubed piny i ŋom, "));
			var input = new List<Block> { block };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), null, null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(5, output.Count);
			Assert.AreEqual("Yecu openyogi ni, ", output[0].GetText(false));
			Assert.IsTrue(output[0].CharacterIs("MAT", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("“Wutye ki mugati adi?” ", output[1].GetText(false));
			Assert.IsFalse(CharacterVerseData.IsCharacterOfType(output[1].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("Gugamo ni, ", output[2].GetText(false));
			Assert.IsTrue(output[2].CharacterIs("MAT", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("“Abiro, ki rec mogo matitino manok.” ", output[3].GetText(false));
			Assert.IsFalse(CharacterVerseData.IsCharacterOfType(output[3].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("Ociko lwak ni gubed piny i ŋom, ", output[4].GetText(false));
			Assert.IsTrue(output[4].CharacterIs("MAT", CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_OneBlockBecomesThree_QuoteInMiddle()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go!» quietly."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.IsTrue(output[0].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("«Go!» ", output[1].GetText(false));
			Assert.IsFalse(CharacterVerseData.IsCharacterOfType(output[1].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("quietly.", output[2].GetText(false));
			Assert.IsTrue(output[2].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_TwoBlocksRemainTwo_NoQuotes()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("See Spot run. "));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("See Jane see Spot run."));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("See Spot run. ", output[0].GetText(false));
			Assert.IsTrue(output[0].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("See Jane see Spot run.", output[1].GetText(false));
			Assert.IsTrue(output[1].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
		}
		[Test]
		public void Parse_TwoBlocksRemainTwo_NoQuotesAndQuotes()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, "));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("«Go!»"));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.IsTrue(output[0].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("«Go!»", output[1].GetText(false));
			Assert.IsFalse(CharacterVerseData.IsCharacterOfType(output[1].CharacterId, CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_TwoBlocksBecomeThree_NoQuotesAndQuotes()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go!» "));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("«Make me!»"));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«Go!» ", output[1].GetText(false));
			Assert.AreEqual("«Make me!»", output[2].GetText(false));
		}
		[Test]
		public void Parse_TwoBlocksBecomeThree_AlreadyBrokenInMiddleOfQuote()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go "));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("west!»"));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«Go ", output[1].GetText(false));
			Assert.AreEqual("west!»", output[2].GetText(false));
		}

		[Test]
		public void Parse_Continuer_UsingStartingMarker()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go!"));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("«Get!»"));
			var input = new List<Block> { block, block2 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal), null, null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«Go!", output[1].GetText(false));
			Assert.AreEqual("«Get!»", output[2].GetText(false));
		}

		[Test]
		public void Parse_Continuer_UsingEndingMarker()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go!"));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("»Get!»"));
			var input = new List<Block> { block, block2 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("«", "»", "»", 1, QuotationMarkingSystemType.Normal), null, null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«Go!", output[1].GetText(false));
			Assert.AreEqual("»Get!»", output[2].GetText(false));
		}

		[Test]
		public void Parse_Continuer_UsingUniqueMarker()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go!"));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("%Get!»"));
			var input = new List<Block> { block, block2 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("«", "»", "%", 1, QuotationMarkingSystemType.Normal), null, null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«Go!", output[1].GetText(false));
			Assert.AreEqual("%Get!»", output[2].GetText(false));
		}

		[Test]
		public void Parse_Continuer_NarratorAfter_FirstLevel()
		{
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("«Get!» Thus he ended."));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«Go!", output[1].GetText(false));
			Assert.AreEqual("«Get!» ", output[2].GetText(false));
			Assert.AreEqual("Thus he ended.", output[3].GetText(false));
		}

		[Test]
		public void Parse_Continuer_QuoteAfter_FirstLevel()
		{
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("«Get!»"));
			var block3 = new Block("p") { IsParagraphStart = true };
			block3.BlockElements.Add(new ScriptText("«No,» she replied."));
			var input = new List<Block> { block, block2, block3 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(5, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«Go!", output[1].GetText(false));
			Assert.AreEqual("«Get!»", output[2].GetText(false));
			Assert.AreEqual("«No,» ", output[3].GetText(false));
			Assert.AreEqual("she replied.", output[4].GetText(false));
		}

		[Test]
		public void Parse_Continuer_NarratorAfter_ThirdLevel()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‹", "›", "«‹", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("«", "»", "«‹«", 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «‹«Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("«‹«Get!»›» Thus he ended."));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«‹«Go!", output[1].GetText(false));
			Assert.AreEqual("«‹«Get!»›» ", output[2].GetText(false));
			Assert.AreEqual("Thus he ended.", output[3].GetText(false));
			Assert.IsTrue(output[3].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_Continuer_HasSpace_NarratorAfter()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‹", "›", "« ‹", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("«", "»", "« ‹ «", 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, « ‹Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("« ‹Get!› »"));
			var block3 = new Block("p") { IsParagraphStart = true };
			block3.BlockElements.Add(new ScriptText("Thus he ended."));
			var input = new List<Block> { block, block2, block3 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("« ‹Go!", output[1].GetText(false));
			Assert.AreEqual("« ‹Get!› »", output[2].GetText(false));
			Assert.AreEqual("Thus he ended.", output[3].GetText(false));
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("LUK", CharacterVerseData.StandardCharacter.Narrator), output[3].CharacterId);
		}

		[Test]
		public void Parse_Continuer_SecondLevelStartsWithFirstLevelContinuer_NarratorAfter()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‹", "›", "«‹", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("«", "»", "«‹«", 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("«‹Get!"));
			var block3 = new Block("p") { IsParagraphStart = true };
			block3.BlockElements.Add(new ScriptText("«No!»"));
			var block4 = new Block("p") { IsParagraphStart = true };
			block4.BlockElements.Add(new ScriptText("Still in quote.›»"));
			var input = new List<Block> { block, block2, block3, block4 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(5, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«Go!", output[1].GetText(false));
			Assert.AreEqual("«‹Get!", output[2].GetText(false));
			Assert.AreEqual("«No!»", output[3].GetText(false));
			Assert.AreEqual("Still in quote.›»", output[4].GetText(false));
			Assert.IsFalse(output[4].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_Continuer_SecondLevelStartsWithFirstLevelContinuer_HasSpaces_NarratorAfter()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‹", "›", "« ‹", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("«", "»", "« ‹ «", 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("« ‹Get!"));
			var block3 = new Block("p") { IsParagraphStart = true };
			block3.BlockElements.Add(new ScriptText("«No!»"));
			var block4 = new Block("p") { IsParagraphStart = true };
			block4.BlockElements.Add(new ScriptText("Still in quote.›»"));
			var input = new List<Block> { block, block2, block3, block4 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(5, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«Go!", output[1].GetText(false));
			Assert.AreEqual("« ‹Get!", output[2].GetText(false));
			Assert.AreEqual("«No!»", output[3].GetText(false));
			Assert.AreEqual("Still in quote.›»", output[4].GetText(false));
			Assert.IsFalse(output[4].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_Continuer_NarratorAfter_ThirdLevel_ContinuerIsOnlyInnermost()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‹", "›", "‹", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("«", "»", "«", 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «‹«Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("«Get!»›» Thus he ended."));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«‹«Go!", output[1].GetText(false));
			Assert.AreEqual("«Get!»›» ", output[2].GetText(false));
			Assert.AreEqual("Thus he ended.", output[3].GetText(false));
		}

		[Test]
		public void Parse_Continuer_QuoteAfter_ThirdLevel()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‹", "›", "«‹", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("«", "»", "«‹«", 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «‹«Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("«‹«Get!»›»"));
			var block3 = new Block("p") { IsParagraphStart = true };
			block3.BlockElements.Add(new ScriptText("«No,» she replied."));
			var input = new List<Block> { block, block2, block3 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(5, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«‹«Go!", output[1].GetText(false));
			Assert.AreEqual("«‹«Get!»›»", output[2].GetText(false));
			Assert.AreEqual("«No,» ", output[3].GetText(false));
			Assert.AreEqual("she replied.", output[4].GetText(false));
		}

		[Test]
		public void Parse_Continuer_QuoteAfter_ThirdLevel_ContinuerIsOnlyInnermost()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‹", "›", "‹", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("«", "»", "«", 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «‹«Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("«Get!»›»"));
			var block3 = new Block("p") { IsParagraphStart = true };
			block3.BlockElements.Add(new ScriptText("«No,» she replied."));
			var input = new List<Block> { block, block2, block3 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(5, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«‹«Go!", output[1].GetText(false));
			Assert.AreEqual("«Get!»›»", output[2].GetText(false));
			Assert.AreEqual("«No,» ", output[3].GetText(false));
			Assert.AreEqual("she replied.", output[4].GetText(false));
		}

		[Test]
		public void Parse_Level3_BlockStartsWithNewThirdLevelQuote()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‹", "›", "‹", 2, QuotationMarkingSystemType.Normal));
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «‹She said, "));
			var block3 = new Block("p") { IsParagraphStart = true };
			block3.BlockElements.Add(new ScriptText("«Get!» rudely.›»"));
			var input = new List<Block> { block, block3 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("«‹She said, ", output[1].GetText(false));
			Assert.AreEqual("«Get!» rudely.›»", output[2].GetText(false));
		}

		[Test]
		public void Parse_StartAndEnd_TwoSameCharacters_QuoteAtBeginning()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("<<Go!>> he said."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("<<Go!>> ", output[0].GetText(false));
			Assert.AreEqual("he said.", output[1].GetText(false));
		}

		[Test]
		public void Parse_StartAndEnd_TwoSameCharacters_QuoteInMiddle()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, <<Go!>> loudly."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("<<Go!>> ", output[1].GetText(false));
			Assert.AreEqual("loudly.", output[2].GetText(false));
		}

		[Test]
		public void Parse_StartAndEnd_TwoSameCharacters_QuoteAtEnd()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, <<Go!>>"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("<<Go!>>", output[1].GetText(false));
		}

		[Test]
		public void Parse_MultipleCharacters_Level1CloseStartsWithLevel2Close_Level1CloseImmediatelyFollowsLevel2Close_ProperlyClosesLevel1Quote()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("<", ">", "<", 2, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, <<She said <Go!> and <Get!> >> and then he finished."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("<<She said <Go!> and <Get!> >> ", output[1].GetText(false));
			Assert.AreEqual("and then he finished.", output[2].GetText(false));
			Assert.AreEqual("narrator-MRK", output[2].CharacterId);
		}

		[Test]
		public void Parse_MultipleCharacters_Level1ContinuerStartsWithLevel2Open_ProperlyClosesLevel1Quote()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("<", ">", "<", 2, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, <<She said <Go!> and "));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("<<Continue>> "));
			var block3 = new Block("p");
			block3.BlockElements.Add(new ScriptText("Not a quote."));
			var input = new List<Block> { block, block2, block3 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("<<She said <Go!> and ", output[1].GetText(false));
			Assert.AreEqual("<<Continue>> ", output[2].GetText(false));
			Assert.AreEqual("Not a quote.", output[3].GetText(false));
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator), output[3].CharacterId);
		}

		[Test]
		public void Parse_MultipleCharacters_3Levels_ProperlyClosesLevel1Quote()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("<<", ">>", "<<", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("<", ">", "<<<", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("<<", ">>", "<<<<<", 3, QuotationMarkingSystemType.Normal));
			var block1 = new Block("p");
			block1.BlockElements.Add(new ScriptText("A gye 'ushu kong le, "));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("<<Udebid ugyang a ma de le: "));
			var block3 = new Block("q1");
			block3.BlockElements.Add(new ScriptText("<Unim a de Atyagi le: <<Be bel kwu-m "));
			var block6 = new Block("q2");
			block6.BlockElements.Add(new ScriptText("abee fe he itang.>> > "));
			var block7 = new Block("m");
			block7.BlockElements.Add(new ScriptText("Gbe Udebid or a ma ko Ukristi le Atyam, ki nya sha ná a, ufe ù ha fel igia ima?>> "));
			var block8 = new Block("p");
			block8.BlockElements.Add(new ScriptText("Undi ken or lè he."));
			var input = new List<Block> { block1, block2, block3, block6, block7, block8 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(6, output.Count);
			Assert.AreEqual("A gye 'ushu kong le, ", output[0].GetText(false));
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator), output[0].CharacterId);
			Assert.AreEqual("<<Udebid ugyang a ma de le: ", output[1].GetText(false));
			Assert.AreNotEqual(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator), output[1].CharacterId);
			Assert.AreEqual("Undi ken or lè he.", output[5].GetText(false));
			Assert.AreEqual(CharacterVerseData.GetStandardCharacterId("MRK", CharacterVerseData.StandardCharacter.Narrator), output[5].CharacterId);
		}

		[Test]
		public void Parse_StartAndEnd_TwoDifferentCharacters_QuoteAtBeginning()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("&*", "^~", "&*", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("&*Go!^~ he said."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("&*Go!^~ ", output[0].GetText(false));
			Assert.AreEqual("he said.", output[1].GetText(false));
		}

		[Test]
		public void Parse_StartAndEnd_TwoDifferentCharacters_QuoteInMiddle()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("&*", "^~", "&*", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, &*Go!^~ loudly."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("&*Go!^~ ", output[1].GetText(false));
			Assert.AreEqual("loudly.", output[2].GetText(false));
		}

		[Test]
		public void Parse_StartAndEnd_TwoDifferentCharacters_QuoteAtEnd()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("&*", "^~", "&*", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, &*Go!^~"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("&*Go!^~", output[1].GetText(false));
		}

		[Test]
		public void Parse_StartAndEnd_ThreeSameCharacters_QuoteAtBeginning()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("<<<", ">>>", "<<<", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("<<<Go!>>> he said."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("<<<Go!>>> ", output[0].GetText(false));
			Assert.AreEqual("he said.", output[1].GetText(false));
		}

		[Test]
		public void Parse_StartAndEnd_ThreeSameCharacters_QuoteInMiddle()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("<<<", ">>>", "<<<", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, <<<Go!>>> loudly."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("<<<Go!>>> ", output[1].GetText(false));
			Assert.AreEqual("loudly.", output[2].GetText(false));
		}

		[Test]
		public void Parse_StartAndEnd_ThreeSameCharacters_QuoteAtEnd()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("<<<", ">>>", "<<<", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, <<<Go!>>>"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("<<<Go!>>>", output[1].GetText(false));
		}

		[Test]
		public void Parse_StartEndSame_QuoteAtEnd()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("\"", "\"", "\"", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, \"Go!\""));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("\"Go!\"", output[1].GetText(false));
		}

		[Test]
		public void Parse_StartEndSame_QuoteAtBeginning()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("\"", "\"", "\"", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("\"Go!\" he said."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("\"Go!\" ", output[0].GetText(false));
			Assert.AreEqual("he said.", output[1].GetText(false));
		}

		[Test]
		public void Parse_StartEndSame_QuoteInMiddle()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("\"", "\"", "\"", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, \"Go!\" quietly."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("\"Go!\" ", output[1].GetText(false));
			Assert.AreEqual("quietly.", output[2].GetText(false));
		}

		[Test]
		public void Parse_StartEndSame_ThreeLevels()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("\"", "\"", "\"", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("'", "'", "'", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("\"", "\"", "\"", 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, \"She said, 'They said, \"No way.\"'\""));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("\"She said, 'They said, \"No way.\"'\"", output[1].GetText(true));
		}

		[Test]
		public void Parse_VerseAtBeginning()
		{
			var block = new Block("p", 5);
			block.BlockElements.Add(new Verse("3"));
			block.BlockElements.Add(new ScriptText("He said, «Go!»"));
			var input = new List<Block> { block };
			Assert.AreEqual(1, input.Count);
			Assert.AreEqual("[3]\u00A0He said, «Go!»", input[0].GetText(true));
			Assert.AreEqual(5, input[0].ChapterNumber);
			Assert.AreEqual(3, input[0].InitialStartVerseNumber);

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("[3]\u00A0He said, ", output[0].GetText(true));
			Assert.AreEqual(5, output[0].ChapterNumber);
			Assert.AreEqual(3, output[0].InitialStartVerseNumber);
			Assert.AreEqual("«Go!»", output[1].GetText(true));
			Assert.AreEqual(5, output[1].ChapterNumber);
			Assert.AreEqual(3, output[1].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_MultipleVersesBeforeQuote()
		{
			var block = new Block("p", 5);
			block.BlockElements.Add(new Verse("3"));
			block.BlockElements.Add(new ScriptText("Matthew tried to learn to fish, but Peter was upset. "));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("He said, «Go back to your tax booth!»"));
			var input = new List<Block> { block };
			Assert.AreEqual(1, input.Count);
			Assert.AreEqual("[3]\u00A0Matthew tried to learn to fish, but Peter was upset. [4]\u00A0He said, «Go back to your tax booth!»", input[0].GetText(true));
			Assert.AreEqual(5, input[0].ChapterNumber);
			Assert.AreEqual(3, input[0].InitialStartVerseNumber);

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("[3]\u00A0Matthew tried to learn to fish, but Peter was upset. [4]\u00A0He said, ", output[0].GetText(true));
			Assert.AreEqual(5, output[0].ChapterNumber);
			Assert.AreEqual(3, output[0].InitialStartVerseNumber);
			Assert.AreEqual("«Go back to your tax booth!»", output[1].GetText(true));
			Assert.AreEqual(5, output[1].ChapterNumber);
			Assert.AreEqual(4, output[1].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_VerseBeforeQuote()
		{
			var block = new Block("p", 6, 2);
			block.BlockElements.Add(new ScriptText("He said, "));
			block.BlockElements.Add(new Verse("3"));
			block.BlockElements.Add(new ScriptText("«Go!»"));
			var input = new List<Block> { block };
			Assert.AreEqual(1, input.Count);
			Assert.AreEqual("He said, [3]\u00A0«Go!»", input[0].GetText(true));
			Assert.AreEqual(6, input[0].ChapterNumber);
			Assert.AreEqual(2, input[0].InitialStartVerseNumber);

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual(6, output[0].ChapterNumber);
			Assert.AreEqual(2, output[0].InitialStartVerseNumber);
			Assert.AreEqual("«Go!»", output[1].GetText(false));
			Assert.AreEqual(6, output[1].ChapterNumber);
			Assert.AreEqual(3, output[1].InitialStartVerseNumber);

			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("[3]\u00A0«Go!»", output[1].GetText(true));
		}

		[Test]
		public void Parse_VerseAfterQuote()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("«Go!» "));
			block.BlockElements.Add(new Verse("3"));
			block.BlockElements.Add(new ScriptText("he said."));
			var input = new List<Block> { block };
			Assert.AreEqual(1, input.Count);
			Assert.AreEqual("«Go!» [3]\u00A0he said.", input[0].GetText(true));

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("«Go!» ", output[0].GetText(false));
			Assert.AreEqual("he said.", output[1].GetText(false));

			Assert.AreEqual("«Go!» ", output[0].GetText(true));
			Assert.AreEqual("[3]\u00A0he said.", output[1].GetText(true));
		}

		[Test]
		public void Parse_VerseWithinQuote()
		{
			var block = new Block("p", 6, 2);
			block.BlockElements.Add(new ScriptText("He said, «Go "));
			block.BlockElements.Add(new Verse("3"));
			block.BlockElements.Add(new ScriptText("west!»"));
			var input = new List<Block> { block };
			Assert.AreEqual(1, input.Count);
			Assert.AreEqual("He said, «Go [3]\u00A0west!»", input[0].GetText(true));
			Assert.AreEqual(6, input[0].ChapterNumber);
			Assert.AreEqual(2, input[0].InitialStartVerseNumber);

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual(6, output[0].ChapterNumber);
			Assert.AreEqual(2, output[0].InitialStartVerseNumber);
			Assert.AreEqual("«Go west!»", output[1].GetText(false));
			Assert.AreEqual(6, output[1].ChapterNumber);
			Assert.AreEqual(2, output[1].InitialStartVerseNumber);

			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("«Go [3]\u00A0west!»", output[1].GetText(true));
		}

		[Test]
		public void Parse_VerseFollowsQuoteEndMarkAndSpace_InitialStartVerseNumberCorrect()
		{
			var block = new Block("p", 1, 1) { IsParagraphStart = true };
			block.BlockElements.Add(new Verse("1"));
			block.BlockElements.Add(new ScriptText("abc "));
			block.BlockElements.Add(new Verse("2"));
			block.BlockElements.Add(new ScriptText("def «ghi» "));
			block.BlockElements.Add(new Verse("3"));
			block.BlockElements.Add(new ScriptText("jkl "));
			var input = new List<Block> { block };

			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("abc def ", output[0].GetText(false));
			Assert.AreEqual("«ghi» ", output[1].GetText(false));
			Assert.AreEqual("jkl ", output[2].GetText(false));
			Assert.AreEqual("[1]\u00A0abc [2]\u00A0def ", output[0].GetText(true));
			Assert.AreEqual(1, output[0].InitialStartVerseNumber);
			Assert.AreEqual("«ghi» ", output[1].GetText(true));
			Assert.AreEqual(2, output[1].InitialStartVerseNumber);
			Assert.AreEqual("[3]\u00A0jkl ", output[2].GetText(true));
			Assert.AreEqual(3, output[2].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_SpaceStaysWithPriorBlock()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go!»"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("«Go!»", output[1].GetText(true));
		}
		[Test]
		public void Parse_PunctuationStaysWithPriorBlock()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("«Go»!! he said."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("«Go»!! ", output[0].GetText(true));
			Assert.AreEqual("he said.", output[1].GetText(true));
		}
		[Test]
		public void Parse_PunctuationStaysWithPriorBlock_AtBlockEnd()
		{
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, «Go»!"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("«Go»!", output[1].GetText(true));
		}

		[Test]
		public void Parse_UsingDifferentQuoteMarks()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("“Go!” he said."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("“Go!” ", output[0].GetText(true));
			Assert.AreEqual("he said.", output[1].GetText(true));
		}

		[Test]
		public void Parse_Level2_BreakOnFirstLevelQuoteOnly()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, “She said, ‘Get lost.’”"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("“She said, ‘Get lost.’”", output[1].GetText(true));
		}

		[Test]
		public void Parse_Level3_BreakOnFirstLevelQuoteOnly()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "“ ‘", 2, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, “She said, ‘They said, “No way.”’”"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("“She said, ‘They said, “No way.”’”", output[1].GetText(true));
		}

		[Test]
		public void Parse_Level3_ContinuesInside_BreakOnFirstLevelQuoteOnly()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "“ ‘", 2, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, “She said, ‘They said, “No way!” rudely.’”"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("“She said, ‘They said, “No way!” rudely.’”", output[1].GetText(true));
		}

		[Test]
		public void Parse_Level3_ContinuesOutside_BreakOnFirstLevelQuoteOnly()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "“ ‘", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("“", "”", "“ ‘ “", 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, “She said, ‘They said, “No way!” rudely,’” politely."));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("“She said, ‘They said, “No way!” rudely,’” ", output[1].GetText(true));
			Assert.AreEqual("politely.", output[2].GetText(true));
		}

		[Test]
		public void Parse_Level3_Level1QuoteFollows_BrokenCorrectly()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‘", "’", "“ ‘", 2, QuotationMarkingSystemType.Normal));
			var block = new Block("p");
			block.BlockElements.Add(new ScriptText("He said, “She said, ‘They said, “No way!” quite rudely.’”"));
			var block2 = new Block("p");
			block2.BlockElements.Add(new ScriptText("He continued, “The end.”"));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(true));
			Assert.AreEqual("“She said, ‘They said, “No way!” quite rudely.’”", output[1].GetText(true));
			Assert.AreEqual("He continued, ", output[2].GetText(true));
			Assert.AreEqual("“The end.”", output[3].GetText(true));
		}

		[Test]
		public void Parse_TitleIntrosChaptersAndExtraBiblicalMaterial_OnlyVerseTextGetsParsedForQuotes()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal));
			var titleBlock = new Block("mt");
			titleBlock.BlockElements.Add(new ScriptText("Gospel of Mark"));
			titleBlock.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.BookOrChapter);
			var introBlock1 = new Block("is");
			introBlock1.BlockElements.Add(new ScriptText("All about Mark"));
			var introBlock2 = new Block("ip");
			introBlock1.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.Intro);
			introBlock2.BlockElements.Add(new ScriptText("Some people say, “Mark is way to short,” but I disagree."));
			introBlock2.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.Intro);
			var chapterBlock = new Block("c");
			chapterBlock.BlockElements.Add(new ScriptText("Chapter 1"));
			chapterBlock.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.BookOrChapter);
			var sectionHeadBlock = new Block("s");
			sectionHeadBlock.BlockElements.Add(new ScriptText("John tells everyone: “The Kingdom of Heaven is at hand”"));
			sectionHeadBlock.SetStandardCharacter("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical);
			var paraBlock = new Block("p");
			paraBlock.BlockElements.Add(new Verse("1"));
			paraBlock.BlockElements.Add(new ScriptText("Jesus said, “Is that John?”"));
			var input = new List<Block> { titleBlock, introBlock1, introBlock2, chapterBlock, sectionHeadBlock, paraBlock };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(7, output.Count);
			Assert.AreEqual("Gospel of Mark", output[0].GetText(true));
			Assert.IsTrue(output[0].CharacterIs("MRK", CharacterVerseData.StandardCharacter.BookOrChapter));
			Assert.AreEqual("All about Mark", output[1].GetText(true));
			Assert.IsTrue(output[1].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Intro));
			Assert.AreEqual("Some people say, “Mark is way to short,” but I disagree.", output[2].GetText(true));
			Assert.IsTrue(output[2].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Intro));
			Assert.AreEqual("Chapter 1", output[3].GetText(true));
			Assert.IsTrue(output[3].CharacterIs("MRK", CharacterVerseData.StandardCharacter.BookOrChapter));
			Assert.AreEqual("John tells everyone: “The Kingdom of Heaven is at hand”", output[4].GetText(true));
			Assert.IsTrue(output[4].CharacterIs("MRK", CharacterVerseData.StandardCharacter.ExtraBiblical));
			Assert.AreEqual("[1]\u00A0Jesus said, ", output[5].GetText(true));
			Assert.IsTrue(output[5].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual("“Is that John?”", output[6].GetText(true));
			Assert.AreEqual(CharacterVerseData.UnknownCharacter, output[6].CharacterId);
		}

		[Test]
		public void Parse_IsParagraphStart()
		{
			var chapterBlock = new Block("c") { IsParagraphStart = true };
			chapterBlock.BlockElements.Add(new ScriptText("Chapter 1"));
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, «Go»!"));
			var input = new List<Block> { chapterBlock, block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("Chapter 1", output[0].GetText(true));
			Assert.AreEqual("He said, ", output[1].GetText(true));
			Assert.AreEqual("«Go»!", output[2].GetText(true));
			Assert.IsTrue(output[0].IsParagraphStart);
			Assert.IsTrue(output[1].IsParagraphStart);
			Assert.IsFalse(output[2].IsParagraphStart);
		}

		[Test]
		public void Parse_IsParagraphStart_BlockStartsWithVerse()
		{
			var block = new Block("q1") { IsParagraphStart = true };
			block.BlockElements.Add(new Verse("23"));
			block.BlockElements.Add(new ScriptText("«Wya dzaʼa zlghafzlgha daghala makwa ta kul snaŋtá zgun ta huɗi, ŋa yatani ta zwaŋa zgun,"));
			var input = new List<Block> { block };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(1, output.Count);
			Assert.AreEqual("[23]\u00A0«Wya dzaʼa zlghafzlgha daghala makwa ta kul snaŋtá zgun ta huɗi, ŋa yatani ta zwaŋa zgun,", output[0].GetText(true));
			Assert.IsTrue(output[0].IsParagraphStart);
		}

		[Test]
		public void Parse_IsParagraphStart_VerseAndQuoteSpansParagraphs()
		{
			var block = new Block("q1") { IsParagraphStart = true };
			block.BlockElements.Add(new Verse("23"));
			block.BlockElements.Add(new ScriptText("«Wya dzaʼa zlghafzlgha daghala makwa ta kul snaŋtá zgun ta huɗi, ŋa yatani ta zwaŋa zgun,"));
			var block2 = new Block("q1") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("ŋa tsanaftá hgani ka Emanuwel,» manda mnay kazlay: Kawadaga Lazglafta nda amu kəʼa ya."));
			var input = new List<Block> { block, block2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("[23]\u00A0«Wya dzaʼa zlghafzlgha daghala makwa ta kul snaŋtá zgun ta huɗi, ŋa yatani ta zwaŋa zgun,", output[0].GetText(true));
			Assert.AreEqual("ŋa tsanaftá hgani ka Emanuwel,» ", output[1].GetText(true));
			Assert.AreEqual("manda mnay kazlay: Kawadaga Lazglafta nda amu kəʼa ya.", output[2].GetText(true));
			Assert.IsTrue(output[0].IsParagraphStart);
			Assert.IsTrue(output[1].IsParagraphStart);
			Assert.IsFalse(output[2].IsParagraphStart);
		}

		[Test]
		public void Parse_QuoteInNewParagraphWithinVerseBridge_NarratorAndOther()
		{
			var block1 = new Block("p", 17, 3, 4);
			block1.BlockElements.Add(new Verse("3-4"));
			block1.BlockElements.Add(new ScriptText("Then Peter said, "));
			var block2 = new Block("q1", 17, 3, 4);
			block2.BlockElements.Add(new ScriptText("«What verse is this?»"));
			var input = new List<Block> { block1, block2 };

			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerse;
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("narrator-MAT", output[0].CharacterId);
			Assert.AreEqual("Peter (Simon)", output[1].CharacterId);
		}

		[Test]
		public void Parse_QuoteInNewParagraphWithinVerseBridge_DifferentCharacters_MarksBothAsAmbiguous()
		{
			var block1 = new Block("p", 6, 7, 9);
			block1.BlockElements.Add(new Verse("7-9"));
			block1.BlockElements.Add(new ScriptText("Philip said, «Surely you can't be serious.»"));
			block1.BlockElements.Add(new ScriptText("Andrew said, «I am serious.»"));
			var block2 = new Block("q1", 6, 7, 9);
			block2.BlockElements.Add(new ScriptText("«And don't call me Shirley.»"));
			var input = new List<Block> { block1, block2 };

			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerse;
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input).Parse().ToList();
			Assert.AreEqual(5, output.Count);
			Assert.AreEqual("narrator-JHN", output[0].CharacterId);
			Assert.AreEqual("Ambiguous", output[1].CharacterId);
			Assert.AreEqual("narrator-JHN", output[2].CharacterId);
			Assert.AreEqual("Ambiguous", output[3].CharacterId);
			Assert.AreEqual("Ambiguous", output[4].CharacterId);
		}

		[Test]
		public void Parse_QuoteInNewParagraphWithinVerseBridge_SameCharacter_MarksBothAsCorrectCharacter()
		{
			var block1 = new Block("p", 1, 19, 20);
			block1.BlockElements.Add(new Verse("19-20"));
			block1.BlockElements.Add(new ScriptText("Peter said, «They don't call him the son of thunder for nothing.»"));
			var block2 = new Block("q1", 1, 19, 20);
			block2.BlockElements.Add(new ScriptText("«Oh, and his brother, too.»"));
			var input = new List<Block> { block1, block2 };

			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerse;
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ACT", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("narrator-ACT", output[0].CharacterId);
			Assert.AreEqual("Peter (Simon)", output[1].CharacterId);
			Assert.AreEqual("Peter (Simon)", output[2].CharacterId);
		}

		[Test]
		public void Parse_QuoteSpansParagraphs()
		{
			var block1 = new Block("p", 1, 23);
			block1.BlockElements.Add(new Verse("23"));
			block1.BlockElements.Add(new ScriptText("«Wya dzaʼa zlghafzlgha daghala makwa ta kul snaŋtá zgun ta huɗi, ŋa yatani ta zwaŋa zgun,"));
			var block2 = new Block("q1", 1, 23);
			block2.BlockElements.Add(new ScriptText("ŋa tsanaftá hgani ka Emanuwel,» manda mnay kazlay: Kawadaga Lazglafta nda amu kəʼa ya."));
			var input = new List<Block> { block1, block2 };

			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerse;
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(3, output.Count);
			Assert.AreEqual("[23]\u00A0«Wya dzaʼa zlghafzlgha daghala makwa ta kul snaŋtá zgun ta huɗi, ŋa yatani ta zwaŋa zgun,", output[0].GetText(true));
			Assert.AreEqual("ŋa tsanaftá hgani ka Emanuwel,» ", output[1].GetText(true));
			Assert.AreEqual("manda mnay kazlay: Kawadaga Lazglafta nda amu kəʼa ya.", output[2].GetText(true));
		}

		[Test]
		public void Parse_AcrossChapter_FindsCorrectCharacters()
		{
			var block1 = new Block("p", 1, 31) { IsParagraphStart = true };
			block1.BlockElements.Add(new Verse("31"));
			block1.BlockElements.Add(new ScriptText("Some text and «a quote» and more text."));
			var blockC = new Block("c", 2, 0) { IsParagraphStart = true };
			blockC.BlockElements.Add(new ScriptText("2"));
			var block2 = new Block("p", 2, 1) { IsParagraphStart = true };
			block2.BlockElements.Add(new Verse("1"));
			block2.BlockElements.Add(new ScriptText("Text in the next chapter and «another quote»"));
			var input = new List<Block> { block1, blockC, block2 };

			ControlCharacterVerseData.TabDelimitedCharacterVerseData = Properties.Resources.TestCharacterVerse;
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "GEN", input).Parse().ToList();
			Assert.AreEqual(6, output.Count);
			Assert.AreEqual("narrator-GEN", output[0].CharacterId);
			Assert.AreEqual("Last in Chapter", output[1].CharacterId);
			Assert.AreEqual("narrator-GEN", output[2].CharacterId);
			Assert.AreEqual("narrator-GEN", output[4].CharacterId);
			Assert.AreEqual("First in Chapter", output[5].CharacterId);
		}

		[Test]
		public void Parse_SpaceAfterVerse_NoEmptyBlock()
		{
			var block1 = new Block("p", 3, 12) { IsParagraphStart = true };
			block1.BlockElements.Add(new ScriptText("«pe, kadi ki acel.» "));
			block1.BlockElements.Add(new Verse("13"));
			block1.BlockElements.Add(new ScriptText(" «Guŋamo doggi calo lyel ma twolo,»"));
			var input = new List<Block> { block1 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ROM", input).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("«pe, kadi ki acel.» ", output[0].GetText(false));
			Assert.AreEqual("«Guŋamo doggi calo lyel ma twolo,»", output[1].GetText(false));
			Assert.AreEqual("«pe, kadi ki acel.» ", output[0].GetText(true));
			Assert.AreEqual("[13]\u00A0«Guŋamo doggi calo lyel ma twolo,»", output[1].GetText(true));
		}

		[Test]
		public void Parse_OpeningParenthesisBeforeLevel2Continuer_NarratorAfter()
		{
			var quoteSystem = new QuoteSystem(new QuotationMark("«", "»", "«", 1, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("‹", "›", "« ‹", 2, QuotationMarkingSystemType.Normal));
			quoteSystem.AllLevels.Add(new QuotationMark("«", "»", "« ‹ «", 3, QuotationMarkingSystemType.Normal));
			var block = new Block("p") { IsParagraphStart = true };
			block.BlockElements.Add(new ScriptText("He said, « ‹Go!"));
			var block2 = new Block("p") { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("(« ‹Get!› »)"));
			var block3 = new Block("p") { IsParagraphStart = true };
			block3.BlockElements.Add(new ScriptText("Thus he ended."));
			var input = new List<Block> { block, block2, block3 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual("He said, ", output[0].GetText(false));
			Assert.AreEqual("« ‹Go!", output[1].GetText(false));
			Assert.AreEqual("(« ‹Get!› »)", output[2].GetText(false));
			Assert.AreEqual("Thus he ended.", output[3].GetText(false));
			Assert.IsTrue(output[3].CharacterIs("LUK", CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_OpeningParenthesisAfterQuote_OpeningParenthesisGoesWithFollowingBlock()
		{
			var block1 = new Block("p", 1, 23) { IsParagraphStart = true };
			block1.BlockElements.Add(new ScriptText("“Na njə́a mənə, wuntə digəlyi dzəgə kə́lə hwi, a njə dzəgə ye zəgwi rə kə za, a mbəlyi dzəgə ka zəgwi tsa Immanuʼel.” (“Immanuʼel” tsa ná, njə́ nee, “tá myi Hyalatəmwə,” əkwə.)"));
			var input = new List<Block> { block1 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), null, null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(5, output.Count);
			Assert.AreEqual("“Na njə́a mənə, wuntə digəlyi dzəgə kə́lə hwi, a njə dzəgə ye zəgwi rə kə za, a mbəlyi dzəgə ka zəgwi tsa Immanuʼel.” ", output[0].GetText(true));
			Assert.AreEqual("(“Immanuʼel” ", output[1].GetText(true));
			Assert.AreEqual("tsa ná, njə́ nee, ", output[2].GetText(true));
			Assert.AreEqual("“tá myi Hyalatəmwə,” ", output[3].GetText(true));
			Assert.AreEqual("əkwə.)", output[4].GetText(true));
		}

		[Test]
		public void Parse_PeriodFollowingClosingQuoteInLastBlock_PeriodGoesWithQuote()
		{
			var block1 = new Block("p", 1, 23) { IsParagraphStart = true };
			block1.BlockElements.Add(new ScriptText("“Na njə́a mənə, wuntə digəlyi dzəgə”."));
			var input = new List<Block> { block1 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), null, null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(1, output.Count);
			Assert.AreEqual("“Na njə́a mənə, wuntə digəlyi dzəgə”.", output[0].GetText(true));
		}

		[Test]
		public void Parse_MultiBlockQuote()
		{
			var block1 = new Block("q1", 1, 23) { IsParagraphStart = true };
			block1.BlockElements.Add(new Verse("23"));
			block1.BlockElements.Add(new ScriptText("«Nen, nyako mo ma peya oŋeyo "));
			var block2 = new Block("q1", 1, 23) { IsParagraphStart = true };
			block2.BlockElements.Add(new ScriptText("laco biyac, "));
			var block3 = new Block("q1", 1, 23) { IsParagraphStart = true };
			block3.BlockElements.Add(new ScriptText("binywalo latin ma laco» "));
			var block4 = new Block("q1", 1, 23) { IsParagraphStart = true };
			block4.BlockElements.Add(new ScriptText("«Gibicako nyiŋe Emmanuel»"));
			var input = new List<Block> { block1, block2, block3, block4 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual(MultiBlockQuote.Start, output[0].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[1].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[2].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, output[3].MultiBlockQuote);
		}

		[Test]
		public void Parse_MultiBlockQuote_AcrossSectionHead()
		{
			var block1 = new Block("p", 5, 16) { IsParagraphStart = true };
			block1.BlockElements.Add(new ScriptText("«Wun bene wubed "));
			var block2 = new Block("s1", 5, 16) { IsParagraphStart = true, CharacterId = CharacterVerseData.GetStandardCharacterId("MAT", CharacterVerseData.StandardCharacter.ExtraBiblical) };
			block2.BlockElements.Add(new ScriptText("Lok ma Yecu opwonyo i kom cik"));
			var block3 = new Block("p", 5, 17) { IsParagraphStart = true };
			block3.BlockElements.Add(new Verse("17"));
			block3.BlockElements.Add(new ScriptText("«Pe wutam ni an "));
			var block4 = new Block("q1", 5, 17) { IsParagraphStart = true };
			block4.BlockElements.Add(new ScriptText("Ada awaco botwu ni»"));
			var input = new List<Block> { block1, block2, block3, block4 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual(MultiBlockQuote.None, output[0].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.None, output[1].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Start, output[2].MultiBlockQuote);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[3].MultiBlockQuote);
		}

		[Test]
		public void Parse_DialogueQuoteAtStartAndNearEnd_OneBlockBecomesTwo()
		{
			var block = new Block("p", 1, 17);
			block.BlockElements.Add(new ScriptText("—Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, —timiayi."));
			var input = new List<Block> { block };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("—Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, ", output[0].GetText(false));
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(string.Empty, output[0].Delivery);
			Assert.AreEqual(1, output[0].ChapterNumber);
			Assert.AreEqual(17, output[0].InitialStartVerseNumber);
			Assert.AreEqual("—timiayi.", output[1].GetText(false));
			Assert.IsTrue(output[1].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(1, output[1].ChapterNumber);
			Assert.AreEqual(17, output[1].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_DialogueQuoteUsingColonWithNoExplicitEnd_OneBlockBecomesTwo()
		{
			var block1 = new Block("p", 14, 6);
			block1.BlockElements.Add(new Verse("6"));
			block1.BlockElements.Add(new ScriptText("Jesús le dijo: Yo soy el camino, y la verdad, y la vida; nadie viene al Padre sino por mí. "));
			block1.BlockElements.Add(new Verse("7"));
			block1.BlockElements.Add(new ScriptText("Si me hubierais conocido, también hubierais conocido a mi Padre; desde ahora le conocéis y le habéis visto."));
			var block2 = new Block("p", 14, 8);
			block2.BlockElements.Add(new Verse("8"));
			block2.BlockElements.Add(new ScriptText("Felipe le dijo: Señor, muéstranos al Padre, y nos basta."));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), ":", null);
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(4, output.Count);
			Assert.AreEqual("[6]\u00A0Jesús le dijo: ", output[0].GetText(true));
			Assert.IsTrue(output[0].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(14, output[0].ChapterNumber);
			Assert.AreEqual(6, output[0].InitialStartVerseNumber);
			Assert.AreEqual("Yo soy el camino, y la verdad, y la vida; nadie viene al Padre sino por mí. [7]\u00A0Si me hubierais conocido, también hubierais conocido a mi Padre; desde ahora le conocéis y le habéis visto.", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(string.Empty, output[1].Delivery);
			Assert.AreEqual(14, output[1].ChapterNumber);
			Assert.AreEqual(6, output[1].InitialStartVerseNumber);

			Assert.AreEqual("[8]\u00A0Felipe le dijo: ", output[2].GetText(true));
			Assert.IsTrue(output[2].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(14, output[2].ChapterNumber);
			Assert.AreEqual(8, output[2].InitialStartVerseNumber);
			Assert.AreEqual("Señor, muéstranos al Padre, y nos basta.", output[3].GetText(true));
			Assert.AreEqual("Philip", output[3].CharacterId);
			Assert.AreEqual(string.Empty, output[3].Delivery);
			Assert.AreEqual(14, output[3].ChapterNumber);
			Assert.AreEqual(8, output[3].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_DialogueQuoteContainingRegularQuote_InnerRegularQuoteIgnored()
		{
			var block = new Block("p", 1, 17);
			block.BlockElements.Add(new ScriptText("—Wína nemartustaram: “Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum,” —timiayi."));
			var input = new List<Block> { block };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("—Wína nemartustaram: “Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum,” ", output[0].GetText(false));
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(string.Empty, output[0].Delivery);
			Assert.AreEqual(1, output[0].ChapterNumber);
			Assert.AreEqual(17, output[0].InitialStartVerseNumber);
			Assert.AreEqual("—timiayi.", output[1].GetText(false));
			Assert.IsTrue(output[1].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(1, output[1].ChapterNumber);
			Assert.AreEqual(17, output[1].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_DialogueQuotesInsideFirstLevelRegularQuotesNotIndicatingChangeOfSpeaker_FirstLevelQuoteBecomesSeparateBlock()
		{
			var block = new Block("p", 1, 1);
			block.BlockElements.Add(new ScriptText("“The following is just an ordinary m-dash — don't treat it as a dialogue quote — okay?”, said the frog."));
			var input = new List<Block> { block };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("“The following is just an ordinary m-dash — don't treat it as a dialogue quote — okay?”, ", output[0].GetText(false));
			Assert.IsTrue(output[0].CharacterIsUnclear());
			Assert.AreEqual(1, output[0].ChapterNumber);
			Assert.AreEqual(1, output[0].InitialStartVerseNumber);
			Assert.AreEqual("said the frog.", output[1].GetText(false));
			Assert.IsTrue(output[1].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(1, output[1].ChapterNumber);
			Assert.AreEqual(1, output[1].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_DialogueQuoteWithNoSentenceEndingPunctuationFollowedByCloseAfterPoetry_QuoteRemainsOpenUntilClosed()
		{
			var block1 = new Block("p", 2, 5);
			block1.BlockElements.Add(new ScriptText("—Belén yaktanam Judá nungkanam nuni akiinatnuitai. Cristo akiinatniuri pachis aarmauka nuwaitai:"));
			var block2 = new Block("q2", 2, 6);
			block2.BlockElements.Add(new Verse("6"));
			block2.BlockElements.Add(new ScriptText("Yus chichaak: “Judá nungkanam yakat Belén tutai mianchauka achatnuitai. Antsu nu yaktanam juun apu akiinatnua nuka Israela weari ainaun inartinuitai. Tura asamtai nu yaktaka chikich yakat Judá nungkanam aa nuna nangkamasang juun atinuitai,”"));
			var block3 = new Block("m", 2, 6);
			block3.BlockElements.Add(new ScriptText("Yus timiayi. Tu aarmawaitai, —tusar aimkarmiayi."));
			var input = new List<Block> { block1, block2, block3 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(4, output.Count);

			Assert.AreEqual("—Belén yaktanam Judá nungkanam nuni akiinatnuitai. Cristo akiinatniuri pachis aarmauka nuwaitai:", output[0].GetText(true));
			Assert.AreEqual("Good Priest", output[0].CharacterId);
			Assert.AreEqual(string.Empty, output[0].Delivery);
			Assert.AreEqual(2, output[0].ChapterNumber);
			Assert.AreEqual(5, output[0].InitialStartVerseNumber);

			Assert.AreEqual("[6]\u00A0Yus chichaak: “Judá nungkanam yakat Belén tutai mianchauka achatnuitai. Antsu nu yaktanam juun apu akiinatnua nuka Israela weari ainaun inartinuitai. Tura asamtai nu yaktaka chikich yakat Judá nungkanam aa nuna nangkamasang juun atinuitai,”", output[1].GetText(true));
			Assert.AreEqual("Good Priest", output[1].CharacterId);
			Assert.AreEqual(2, output[1].ChapterNumber);
			Assert.AreEqual(6, output[1].InitialStartVerseNumber);

			Assert.AreEqual("Yus timiayi. Tu aarmawaitai, ", output[2].GetText(true));
			Assert.AreEqual("Good Priest", output[2].CharacterId);
			Assert.AreEqual(2, output[2].ChapterNumber);
			Assert.AreEqual(6, output[2].InitialStartVerseNumber);

			Assert.AreEqual("—tusar aimkarmiayi.", output[3].GetText(true));
			Assert.IsTrue(output[3].CharacterIs("MAT", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(2, output[3].ChapterNumber);
			Assert.AreEqual(6, output[3].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_DialogueQuoteWithNoSentenceEndingPunctuationFollowedByCloseAfterPoetry_SentenceEndingWithinPoetry_QuoteRemainsOpenUntilClosed()
		{
			var block1 = new Block("p", 2, 5);
			block1.BlockElements.Add(new ScriptText("—Quote:"));
			var block2 = new Block("q2", 2, 6);
			block2.BlockElements.Add(new Verse("6"));
			block2.BlockElements.Add(new ScriptText("Poetry stuff. "));
			var block3 = new Block("m", 2, 6);
			block3.BlockElements.Add(new ScriptText("More poetry stuff —back to narrator."));
			var input = new List<Block> { block1, block2, block3 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(4, output.Count);

			Assert.AreEqual("—Quote:", output[0].GetText(true));
			Assert.AreEqual("Good Priest", output[0].CharacterId);
			Assert.AreEqual(string.Empty, output[0].Delivery);
			Assert.AreEqual(2, output[0].ChapterNumber);
			Assert.AreEqual(5, output[0].InitialStartVerseNumber);

			Assert.AreEqual("[6]\u00A0Poetry stuff. ", output[1].GetText(true));
			Assert.AreEqual("Good Priest", output[1].CharacterId);
			Assert.AreEqual(2, output[1].ChapterNumber);
			Assert.AreEqual(6, output[1].InitialStartVerseNumber);

			Assert.AreEqual("More poetry stuff ", output[2].GetText(true));
			Assert.AreEqual("Good Priest", output[2].CharacterId);
			Assert.AreEqual(2, output[2].ChapterNumber);
			Assert.AreEqual(6, output[2].InitialStartVerseNumber);

			Assert.AreEqual("—back to narrator.", output[3].GetText(true));
			Assert.IsTrue(output[3].CharacterIs("MAT", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(2, output[3].ChapterNumber);
			Assert.AreEqual(6, output[3].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_DialogueQuoteWithNoExplicitEnd_QuoteClosedByEndOfParagraph()
		{
			var block1 = new Block("p", 1, 17);
			block1.BlockElements.Add(new Verse("17"));
			block1.BlockElements.Add(new ScriptText("Quia joˈ tso Jesús nda̱a̱na:"));
			var block2 = new Block("p", 1, 17);
			block2.BlockElements.Add(new ScriptText("—Quioˈyoˈ ñˈeⁿndyo̱ ndoˈ ja nntsˈaa na nlatjomˈyoˈ nnˈaⁿ tachii cweˈ calcaa."));
			var block3 = new Block("m", 1, 18);
			block3.BlockElements.Add(new Verse("18"));
			block3.BlockElements.Add(new ScriptText("Joona mañoomˈ ˈndyena lquiˈ ˈnaaⁿna. Tyˈena ñˈeⁿñê."));
			var input = new List<Block> { block1, block2, block3 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("[17]\u00A0Quia joˈ tso Jesús nda̱a̱na:", output[0].GetText(true));
			Assert.IsTrue(output[0].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(1, output[0].ChapterNumber);
			Assert.AreEqual(17, output[0].InitialStartVerseNumber);

			Assert.AreEqual("—Quioˈyoˈ ñˈeⁿndyo̱ ndoˈ ja nntsˈaa na nlatjomˈyoˈ nnˈaⁿ tachii cweˈ calcaa.", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual(1, output[1].ChapterNumber);
			Assert.AreEqual(17, output[1].InitialStartVerseNumber);

			Assert.AreEqual("[18]\u00A0Joona mañoomˈ ˈndyena lquiˈ ˈnaaⁿna. Tyˈena ñˈeⁿñê.", output[2].GetText(true));
			Assert.IsTrue(output[2].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(1, output[2].ChapterNumber);
			Assert.AreEqual(18, output[2].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_DialogueQuoteWithPotentialContinuer_EndedByQuotationDash()
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("—Wikia tuke pujutan sukartin asan. "));
			var block2 = new Block("p", 6, 49);
			block2.BlockElements.Add(new Verse("49"));
			block2.BlockElements.Add(new ScriptText("“Nintimrataram, —Jesús timiayi."));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("—Wikia tuke pujutan sukartin asan. ", output[0].GetText(true));
			Assert.AreEqual("Jesus", output[0].CharacterId);

			Assert.AreEqual("[49]\u00A0“Nintimrataram, ", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);

			Assert.AreEqual("—Jesús timiayi.", output[2].GetText(true));
			Assert.IsTrue(output[2].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_DialogueQuoteWithPotentialContinuerOverMultipleParagraphs_EndedByQuotationDash()
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("—Wikia tuke pujutan sukartin asan. "));
			var block2 = new Block("p", 6, 49);
			block2.BlockElements.Add(new Verse("49"));
			block2.BlockElements.Add(new ScriptText("“Nintimrataram,” "));
			var block3 = new Block("p", 6, 50);
			block3.BlockElements.Add(new Verse("50"));
			block3.BlockElements.Add(new ScriptText("“Antsu yurumkan, —Jesús timiayi."));
			var input = new List<Block> { block1, block2, block3 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(4, output.Count);

			Assert.AreEqual("—Wikia tuke pujutan sukartin asan. ", output[0].GetText(true));
			Assert.AreEqual("Jesus", output[0].CharacterId);

			Assert.AreEqual("[49]\u00A0“Nintimrataram,” ", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);

			Assert.AreEqual("[50]\u00A0“Antsu yurumkan, ", output[2].GetText(true));
			Assert.AreEqual("Jesus", output[2].CharacterId);

			Assert.AreEqual("—Jesús timiayi.", output[3].GetText(true));
			Assert.IsTrue(output[3].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_DialogueQuoteWithPotentialContinuer_EndedByFirstLevelEnd()
		{
			var block1 = new Block("p", 6, 48);
			block1.BlockElements.Add(new ScriptText("—Wikia tuke pujutan sukartin asan. "));
			var block2 = new Block("p", 6, 49);
			block2.BlockElements.Add(new Verse("49"));
			block2.BlockElements.Add(new ScriptText("“Nintimrataram,” Jesús timiayi."));
			var input = new List<Block> { block1, block2 };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "—", "—");
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "JHN", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(3, output.Count);

			Assert.AreEqual("—Wikia tuke pujutan sukartin asan. ", output[0].GetText(true));
			Assert.AreEqual("Jesus", output[0].CharacterId);

			Assert.AreEqual("[49]\u00A0“Nintimrataram,” ", output[1].GetText(true));
			Assert.AreEqual("Jesus", output[1].CharacterId);

			Assert.AreEqual("Jesús timiayi.", output[2].GetText(true));
			Assert.IsTrue(output[2].CharacterIs("JHN", CharacterVerseData.StandardCharacter.Narrator));
		}

		[Test]
		public void Parse_DialogueQuoteUsesHyphen_ParsesCorrectly()
		{
			var block = new Block("p", 1, 17);
			block.BlockElements.Add(new ScriptText("-Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, -timiayi."));
			var input = new List<Block> { block };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "-", "-");
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("-Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, ", output[0].GetText(false));
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(string.Empty, output[0].Delivery);
			Assert.AreEqual(1, output[0].ChapterNumber);
			Assert.AreEqual(17, output[0].InitialStartVerseNumber);
			Assert.AreEqual("-timiayi.", output[1].GetText(false));
			Assert.IsTrue(output[1].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(1, output[1].ChapterNumber);
			Assert.AreEqual(17, output[1].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_DialogueQuoteUsesTwoHyphens_ParsesCorrectly()
		{
			var block = new Block("p", 1, 17);
			block.BlockElements.Add(new ScriptText("--Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, --timiayi."));
			var input = new List<Block> { block };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "--", "--");
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("--Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, ", output[0].GetText(false));
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(string.Empty, output[0].Delivery);
			Assert.AreEqual(1, output[0].ChapterNumber);
			Assert.AreEqual(17, output[0].InitialStartVerseNumber);
			Assert.AreEqual("--timiayi.", output[1].GetText(false));
			Assert.IsTrue(output[1].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(1, output[1].ChapterNumber);
			Assert.AreEqual(17, output[1].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_DialogueQuoteUsesEndash_ParsesCorrectly()
		{
			var block = new Block("p", 1, 17);
			block.BlockElements.Add(new ScriptText("–Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, –timiayi."));
			var input = new List<Block> { block };
			var quoteSystem = QuoteSystem.GetOrCreateQuoteSystem(new QuotationMark("“", "”", "“", 1, QuotationMarkingSystemType.Normal), "–", "–");
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input, quoteSystem).Parse().ToList();
			Assert.AreEqual(2, output.Count);
			Assert.AreEqual("–Wína nemartustaram. Turaram namak achiarme nunisrumek aints ainau wína chichamur ujakmintrum, ", output[0].GetText(false));
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(string.Empty, output[0].Delivery);
			Assert.AreEqual(1, output[0].ChapterNumber);
			Assert.AreEqual(17, output[0].InitialStartVerseNumber);
			Assert.AreEqual("–timiayi.", output[1].GetText(false));
			Assert.IsTrue(output[1].CharacterIs("MRK", CharacterVerseData.StandardCharacter.Narrator));
			Assert.AreEqual(1, output[1].ChapterNumber);
			Assert.AreEqual(17, output[1].InitialStartVerseNumber);
		}

		[Test]
		public void Parse_VerseBridge_CharacterInEachVerse_ResultIsAmbiguous()
		{
			var block1 = new Block("p", 9, 27, 28);
			block1.BlockElements.Add(new Verse("27-28"));
			block1.BlockElements.Add(new ScriptText("«Quote.»"));
			var input = new List<Block> { block1 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "ROM", input).Parse().ToList();
			Assert.AreEqual(1, output.Count);
			Assert.AreEqual(CharacterVerseData.AmbiguousCharacter, output[0].CharacterId);
		}

		[Test]
		public void Parse_MultiBlockQuote_TwoCharacters_SetToAmbiguous()
		{
			var block1 = new Block("p", 2, 7);
			block1.BlockElements.Add(new Verse("7"));
			block1.BlockElements.Add(new ScriptText("«Quote."));
			var block2 = new Block("p", 2, 8);
			block2.BlockElements.Add(new Verse("8"));
			block2.BlockElements.Add(new ScriptText("«Continuation of quote by a second speaker."));
			var input = new List<Block> { block1, block2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			// Validate environment
			Assert.AreEqual("teachers of religious law/Pharisees", ControlCharacterVerseData.Singleton.GetCharacters("MRK", 2, 7).Select(cv => cv.Character).Single());
			Assert.AreEqual("Jesus", ControlCharacterVerseData.Singleton.GetCharacters("MRK", 2, 8).Select(cv => cv.Character).Single());

			Assert.AreEqual(2, output.Count);
			Assert.AreEqual(MultiBlockQuote.Start, output[0].MultiBlockQuote);
			Assert.AreEqual(CharacterVerseData.AmbiguousCharacter, output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[1].MultiBlockQuote);
			Assert.AreEqual(CharacterVerseData.AmbiguousCharacter, output[1].CharacterId);
		}

		[Test]
		public void Parse_MultiBlockQuote_TwoCharactersAndAmbiguous_SetToAmbiguous()
		{
			var block1 = new Block("p", 19, 16);
			block1.BlockElements.Add(new Verse("16"));
			block1.BlockElements.Add(new ScriptText("«Quote."));
			var block2 = new Block("p", 19, 17);
			block2.BlockElements.Add(new Verse("17"));
			block2.BlockElements.Add(new ScriptText("«Continuation of quote by a second speaker."));
			var block3 = new Block("p", 19, 18);
			block3.BlockElements.Add(new Verse("18"));
			block3.BlockElements.Add(new ScriptText("«Continuation of quote by ambiguous speaker."));
			var input = new List<Block> { block1, block2, block3 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();

			// Validate environment
			Assert.AreEqual("ruler, a certain=man, rich young", ControlCharacterVerseData.Singleton.GetCharacters("MAT", 19, 16).Select(cv => cv.Character).Single());
			Assert.AreEqual("Jesus", ControlCharacterVerseData.Singleton.GetCharacters("MAT", 19, 17).Select(cv => cv.Character).Single());
			Assert.IsTrue(ControlCharacterVerseData.Singleton.GetCharacters("MAT", 19, 18).Select(cv => cv.Character).Contains("Jesus"));
			Assert.IsTrue(ControlCharacterVerseData.Singleton.GetCharacters("MAT", 19, 18).Select(cv => cv.Character).Contains("ruler, a certain=man, rich young"));

			Assert.AreEqual(3, output.Count);
			Assert.AreEqual(MultiBlockQuote.Start, output[0].MultiBlockQuote);
			Assert.AreEqual(CharacterVerseData.AmbiguousCharacter, output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[1].MultiBlockQuote);
			Assert.AreEqual(CharacterVerseData.AmbiguousCharacter, output[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[2].MultiBlockQuote);
			Assert.AreEqual(CharacterVerseData.AmbiguousCharacter, output[2].CharacterId);
		}

		[Test]
		public void Parse_MultiBlockQuote_TwoCharactersAndUnknown_SetToAmbiguous()
		{
			var block1 = new Block("p", 2, 7);
			block1.BlockElements.Add(new Verse("7"));
			block1.BlockElements.Add(new ScriptText("«Quote."));
			var block2 = new Block("p", 2, 8);
			block2.BlockElements.Add(new Verse("8"));
			block2.BlockElements.Add(new ScriptText("«Continuation of quote by a second speaker."));
			var block3 = new Block("p", 2, 9);
			block3.BlockElements.Add(new Verse("9"));
			block3.BlockElements.Add(new ScriptText("«Continuation of quote by an unknown speaker."));
			var input = new List<Block> { block1, block2, block3 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			// Validate environment
			Assert.AreEqual("teachers of religious law/Pharisees", ControlCharacterVerseData.Singleton.GetCharacters("MRK", 2, 7).Select(cv => cv.Character).Single());
			Assert.AreEqual("Jesus", ControlCharacterVerseData.Singleton.GetCharacters("MRK", 2, 8).Select(cv => cv.Character).Single());
			Assert.IsFalse(ControlCharacterVerseData.Singleton.GetCharacters("MRK", 2, 9).Any());

			Assert.AreEqual(3, output.Count);
			Assert.AreEqual(MultiBlockQuote.Start, output[0].MultiBlockQuote);
			Assert.AreEqual(CharacterVerseData.AmbiguousCharacter, output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[1].MultiBlockQuote);
			Assert.AreEqual(CharacterVerseData.AmbiguousCharacter, output[1].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[2].MultiBlockQuote);
			Assert.AreEqual(CharacterVerseData.AmbiguousCharacter, output[2].CharacterId);
		}

		[Test]
		public void Parse_MultiBlockQuote_AmbiguousAndCharacter_SetToCharacter()
		{
			var block1 = new Block("p", 19, 17);
			block1.BlockElements.Add(new Verse("17"));
			block1.BlockElements.Add(new ScriptText("«Quote."));
			var block2 = new Block("p", 19, 18);
			block2.BlockElements.Add(new Verse("18"));
			block2.BlockElements.Add(new ScriptText("«Continuation of quote by ambiguous speaker."));
			var input = new List<Block> { block1, block2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();

			// Validate environment
			Assert.AreEqual("Jesus", ControlCharacterVerseData.Singleton.GetCharacters("MAT", 19, 17).Select(cv => cv.Character).Single());
			Assert.IsTrue(ControlCharacterVerseData.Singleton.GetCharacters("MAT", 19, 18).Select(cv => cv.Character).Contains("Jesus"));
			Assert.IsTrue(ControlCharacterVerseData.Singleton.GetCharacters("MAT", 19, 18).Select(cv => cv.Character).Contains("ruler, a certain=man, rich young"));

			Assert.AreEqual(2, output.Count);
			Assert.AreEqual(MultiBlockQuote.Start, output[0].MultiBlockQuote);
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[1].MultiBlockQuote);
			Assert.AreEqual("Jesus", output[1].CharacterId);
		}

		[Test]
		public void Parse_MultiBlockQuote_UnknownAndCharacter_SetToCharacter()
		{
			var block1 = new Block("p", 19, 8);
			block1.BlockElements.Add(new Verse("8"));
			block1.BlockElements.Add(new ScriptText("«Quote."));
			var block2 = new Block("p", 19, 9);
			block2.BlockElements.Add(new Verse("9"));
			block2.BlockElements.Add(new ScriptText("«Continuation of quote by unknown speaker."));
			var input = new List<Block> { block1, block2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();

			// Validate environment
			Assert.AreEqual("Jesus", ControlCharacterVerseData.Singleton.GetCharacters("MAT", 19, 8).Select(cv => cv.Character).Single());
			Assert.IsFalse(ControlCharacterVerseData.Singleton.GetCharacters("MAT", 19, 9).Any());

			Assert.AreEqual(2, output.Count);
			Assert.AreEqual(MultiBlockQuote.Start, output[0].MultiBlockQuote);
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[1].MultiBlockQuote);
			Assert.AreEqual("Jesus", output[1].CharacterId);
		}

		[Test]
		public void Parse_MultiBlockQuote_AmbiguousAndUnknown_SetToAmbiguous()
		{
			var block1 = new Block("p", 19, 18);
			block1.BlockElements.Add(new Verse("18"));
			block1.BlockElements.Add(new ScriptText("«Ambiguous quote."));
			var block2 = new Block("p", 19, 19);
			block2.BlockElements.Add(new Verse("19"));
			block2.BlockElements.Add(new ScriptText("«Continuation of quote by unknown speaker."));
			var input = new List<Block> { block1, block2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MAT", input).Parse().ToList();

			// Validate environment
			Assert.IsTrue(ControlCharacterVerseData.Singleton.GetCharacters("MAT", 19, 18).Select(cv => cv.Character).Contains("Jesus"));
			Assert.IsTrue(ControlCharacterVerseData.Singleton.GetCharacters("MAT", 19, 18).Select(cv => cv.Character).Contains("ruler, a certain=man, rich young"));
			Assert.IsFalse(ControlCharacterVerseData.Singleton.GetCharacters("MAT", 19, 19).Any());

			Assert.AreEqual(2, output.Count);
			Assert.AreEqual(MultiBlockQuote.Start, output[0].MultiBlockQuote);
			Assert.AreEqual(CharacterVerseData.AmbiguousCharacter, output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[1].MultiBlockQuote);
			Assert.AreEqual(CharacterVerseData.AmbiguousCharacter, output[1].CharacterId);
		}

		[Test]
		public void Parse_MultiBlockQuote_DifferentDeliveries_SetChangeOfDelivery()
		{
			var block1 = new Block("p", 16, 16);
			block1.BlockElements.Add(new Verse("16"));
			block1.BlockElements.Add(new ScriptText("«Quote."));
			var block2 = new Block("p", 16, 17);
			block2.BlockElements.Add(new Verse("17"));
			block2.BlockElements.Add(new ScriptText("«Continuation of quote by same speaker and different delivery."));
			var input = new List<Block> { block1, block2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "MRK", input).Parse().ToList();

			// Validate environment
			Assert.AreEqual("Jesus", ControlCharacterVerseData.Singleton.GetCharacters("MRK", 16, 16).Select(cv => cv.Character).Single());
			Assert.AreEqual("giving orders", ControlCharacterVerseData.Singleton.GetCharacters("MRK", 16, 16).Select(cv => cv.Delivery).Single());
			Assert.AreEqual("Jesus", ControlCharacterVerseData.Singleton.GetCharacters("MRK", 16, 17).Select(cv => cv.Character).Single());
			Assert.AreEqual("", ControlCharacterVerseData.Singleton.GetCharacters("MRK", 16, 17).Select(cv => cv.Delivery).Single());

			Assert.AreEqual(2, output.Count);
			Assert.AreEqual(MultiBlockQuote.Start, output[0].MultiBlockQuote);
			Assert.AreEqual("Jesus", output[0].CharacterId);
			Assert.AreEqual("giving orders", output[0].Delivery);
			Assert.AreEqual(MultiBlockQuote.ChangeOfDelivery, output[1].MultiBlockQuote);
			Assert.AreEqual("Jesus", output[1].CharacterId);
			Assert.AreEqual("", output[1].Delivery);
		}

		[Test]
		public void Parse_MultiBlockQuote_DifferentDeliveries_AmbiguousAndCharacter_SetToCharacter()
		{
			var block1 = new Block("p", 1, 28);
			block1.BlockElements.Add(new Verse("28"));
			block1.BlockElements.Add(new ScriptText("«Quote."));
			var block2 = new Block("p", 1, 29);
			block2.BlockElements.Add(new Verse("29"));
			block2.BlockElements.Add(new ScriptText("«Continuation of quote by ambiguous speaker."));
			var input = new List<Block> { block1, block2 };
			IList<Block> output = new QuoteParser(ControlCharacterVerseData.Singleton, "LUK", input).Parse().ToList();

			// Validate environment
			Assert.AreEqual("angel of the LORD, an", ControlCharacterVerseData.Singleton.GetCharacters("LUK", 1, 28).Select(cv => cv.Character).Single());
			Assert.AreEqual("to Mary", ControlCharacterVerseData.Singleton.GetCharacters("LUK", 1, 28).Select(cv => cv.Delivery).Single());
			Assert.AreEqual(1, ControlCharacterVerseData.Singleton.GetCharacters("LUK", 1, 29).Count(c => c.Character == "angel of the LORD, an" && c.Delivery == ""));
			Assert.IsTrue(ControlCharacterVerseData.Singleton.GetCharacters("LUK", 1, 29).Select(cv => cv.Character).Contains("Mary (Jesus' mother)"));

			Assert.AreEqual(2, output.Count);
			Assert.AreEqual(MultiBlockQuote.Start, output[0].MultiBlockQuote);
			Assert.AreEqual("angel of the LORD, an", output[0].CharacterId);
			Assert.AreEqual(MultiBlockQuote.Continuation, output[1].MultiBlockQuote);
			Assert.AreEqual("angel of the LORD, an", output[1].CharacterId);
		}
	}
}
