﻿using Glyssen;
using NUnit.Framework;

namespace GlyssenTests
{
	[TestFixture]
	class ProjectExport
	{
		[Test]
		public void GetExportLineForBlock_VerseAndTextElements_ExpectedColumnsIncludingJoinedText()
		{
			var block = new Block("p", 4);
			block.IsParagraphStart = true;
			block.CharacterId = "Fred";
			block.Delivery = "With great gusto and quivering frustration";
			block.BlockElements.Add(new Verse("1"));
			block.BlockElements.Add(new ScriptText("Text of verse one. "));
			block.BlockElements.Add(new Verse("2"));
			block.BlockElements.Add(new ScriptText("Text of verse two."));

			int textLength = "Text of verse one. ".Length + "Text of verse two.".Length;
			Assert.AreEqual("0\tp\tMRK\t4\t1\tFred\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t" + textLength,
				Glyssen.ProjectExport.GetExportLineForBlock(block, 0, "MRK"));
			Assert.AreEqual("0\tActorGuy1\tp\tMRK\t4\t1\tFred\tWith great gusto and quivering frustration\t[1]\u00A0Text of verse one. [2]\u00A0Text of verse two.\t" + textLength,
				Glyssen.ProjectExport.GetExportLineForBlock(block, 0, "MRK", "ActorGuy1"));
		}

		[Test]
		public void GetExportLineForBlock_TextBeginsMidVerse_ResultHasCorrectVerseInfo()
		{
			var block = new Block("p", 4, 3);
			block.BlockElements.Add(new ScriptText("Text of verse three, part two. "));
			block.BlockElements.Add(new Verse("4"));
			block.BlockElements.Add(new ScriptText("Text of verse four. "));
			block.BlockElements.Add(new Verse("5"));
			block.BlockElements.Add(new ScriptText("Text of verse five."));

			int textLength = "Text of verse three, part two. ".Length + "Text of verse four. ".Length + "Text of verse five.".Length;
			Assert.AreEqual("0\tp\tMRK\t4\t3\t\t\tText of verse three, part two. [4]\u00A0Text of verse four. [5]\u00A0Text of verse five.\t" + textLength,
				Glyssen.ProjectExport.GetExportLineForBlock(block, 0, "MRK"));
			Assert.AreEqual("0\tActorGuy1\tp\tMRK\t4\t3\t\t\tText of verse three, part two. [4]\u00A0Text of verse four. [5]\u00A0Text of verse five.\t" + textLength,
				Glyssen.ProjectExport.GetExportLineForBlock(block, 0, "MRK", "ActorGuy1"));
		}
	}
}
