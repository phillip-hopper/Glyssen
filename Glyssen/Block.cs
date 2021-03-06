﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Serialization;
using Glyssen.Character;
using SIL.Xml;

namespace Glyssen
{
	[XmlRoot("block")]
	public class Block
	{
		/// <summary>Blocks which has not yet been parsed to identify contents/character</summary>
		public static readonly string NotSet = null;

		public static readonly int NotSplit = -1;

		public const string kCssFrame = "body{{font-family:{0};font-size:{1}pt}}" +
						".right-to-left{{direction:rtl}}" +
						".scripttext {{display:inline}}";

		/// <summary>Random string which will (hopefully) never appear in real text</summary>
		private const string kAwooga = "^~^";

		private int m_initialStartVerseNumber;
		private int m_initialEndVerseNumber;
		private int m_chapterNumber;
		private string m_characterIdInScript;

		public Block()
		{
			// Needed for deserialization
			SplitId = NotSplit;
		}

		public Block(string styleTag, int chapterNum = 0, int initialStartVerseNum = 0, int initialEndVerseNum = 0) : this()
		{
			StyleTag = styleTag;
			BlockElements = new List<BlockElement>();
			ChapterNumber = chapterNum;
			InitialStartVerseNumber = initialStartVerseNum;
			InitialEndVerseNumber = initialEndVerseNum;
		}

		public Block Clone()
		{
			var newBlock = (Block)MemberwiseClone();
			newBlock.BlockElements = new List<BlockElement>(BlockElements.Count);
			foreach (var blockElement in BlockElements)
				newBlock.BlockElements.Add(blockElement.Clone());
			return newBlock;
		}

		[XmlAttribute("style")]
		public string StyleTag { get; set; }

		[XmlAttribute("paragraphStart")]
		[DefaultValue(false)]
		public bool IsParagraphStart { get; set; }

		[XmlAttribute("chapter")]
		public int ChapterNumber
		{
			get
			{
				if (m_chapterNumber == 0)
				{
					if (InitialStartVerseNumber > 0 || BlockElements.Any(b => b is Verse))
						m_chapterNumber = 1;
				}
				return m_chapterNumber;
			}
			set { m_chapterNumber = value; }
		}

		[XmlAttribute("initialStartVerse")]
		public int InitialStartVerseNumber
		{
			get
			{
				if (m_initialStartVerseNumber == 0)
				{
					var leadingVerseElement = BlockElements.FirstOrDefault() as Verse;
					if (leadingVerseElement != null)
					{
						m_initialStartVerseNumber = leadingVerseElement.StartVerse;
					}
					else if (BlockElements.Any(b => b is Verse))
					{
						m_initialStartVerseNumber = 1;
					}
				}
				return m_initialStartVerseNumber;
			}
			set { m_initialStartVerseNumber = value; }
		}

		public int LastVerse
		{
			get
			{
				var lastVerse = BlockElements.OfType<Verse>().LastOrDefault();
				if (lastVerse == null)
					return m_initialEndVerseNumber > 0 ? m_initialEndVerseNumber : m_initialStartVerseNumber;
				return lastVerse.EndVerse;
			}
		}

		[XmlAttribute("initialEndVerse")]
		[DefaultValue(0)]
		public int InitialEndVerseNumber {
			get { return m_initialEndVerseNumber; }
			set { m_initialEndVerseNumber = m_initialStartVerseNumber == value ? 0 : value; }
		}

		/// <summary>
		/// This is the character ID assigned by Glyssen or selected by the user during Phase 1 (protoscript).
		/// Do not use this in Phase 2 (actor assignment); Instead, use CharacterIdInScript.
		/// This setter does not update the CharacterIdInScript value. Therefore, it can be used for
		/// deserialization, cloning (e.g., when applying user decisions), and setting character IDs that
		/// are guaranteed not to represent multiple characters (e.g., Standard characters). In other contexts,
		/// use the SetCharacterIdAndCharacterIdInScript method.
		/// </summary>
		[XmlAttribute("characterId")]
		public string CharacterId { get; set; }

		[XmlAttribute("characterIdOverrideForScript")]
		public string CharacterIdOverrideForScript
		{
			get { return m_characterIdInScript; }
			set { CharacterIdInScript = value; }
		}

		[XmlIgnore]
		public string CharacterIdInScript
		{
			get { return m_characterIdInScript ?? CharacterId; }
			set { m_characterIdInScript = value; }
		}

		[XmlAttribute("delivery")]
		public string Delivery { get; set; }

		[XmlAttribute("userConfirmed")]
		[DefaultValue(false)]
		public bool UserConfirmed { get; set; }

		[XmlAttribute("multiBlockQuote")]
		[DefaultValue(MultiBlockQuote.None)]
		public MultiBlockQuote MultiBlockQuote { get; set; }

		[XmlAttribute("splitId")]
		[DefaultValue(-1)]
		public int SplitId { get; set; }

		[XmlElement(Type = typeof (ScriptText), ElementName = "text")]
		[XmlElement(Type = typeof (Verse), ElementName = "verse")]
		public List<BlockElement> BlockElements { get; set; }

		public bool CharacterIsStandard
		{
			get { return CharacterVerseData.IsCharacterStandard(CharacterId); }
		}

		public string GetText(bool includeVerseNumbers)
		{
			StringBuilder bldr = new StringBuilder();

			foreach (var blockElement in BlockElements)
			{
				Verse verse = blockElement as Verse;
				if (verse != null)
				{
					if (includeVerseNumbers)
					{
						bldr.Append("[");
						bldr.Append(verse.Number);
						bldr.Append("]\u00A0");
					}
				}
				else
				{
					ScriptText text = blockElement as ScriptText;
					if (text != null)
						bldr.Append(text.Content);
				}
			}
			return bldr.ToString();
		}

		public string InitialVerseNumberOrBridge
		{
			get
			{
				return InitialEndVerseNumber == 0 ? InitialStartVerseNumber.ToString(CultureInfo.InvariantCulture) :
					InitialStartVerseNumber + "-" + InitialEndVerseNumber;
			}
		}

		public string GetTextAsHtml(bool showVerseNumbers, bool rightToLeftScript, string verseToInsertExtra = null, int offsetToInsertExtra = -1, string extra = null)
		{
			StringBuilder bldr = new StringBuilder();

			var currVerse = InitialVerseNumberOrBridge;

			foreach (var blockElement in BlockElements)
			{
				Verse verse = blockElement as Verse;
				if (verse != null)
				{
					if (showVerseNumbers)
					{
						bldr.Append("<sup>");
						if (rightToLeftScript)
							bldr.Append("&rlm;");
						bldr.Append(verse.Number);
						bldr.Append("&#160;");
						if (rightToLeftScript)
							bldr.Append("&rlm;");
						bldr.Append("</sup>");
					}
					currVerse = verse.Number;
				}
				else
				{
					ScriptText text = blockElement as ScriptText;
					if (text != null)
					{
						var encodedContent = HttpUtility.HtmlEncode(text.Content);
						if (verseToInsertExtra == currVerse)
						{
							if (offsetToInsertExtra == BookScript.kSplitAtEndOfVerse)
								offsetToInsertExtra = encodedContent.Length;
							if (offsetToInsertExtra < 0 || offsetToInsertExtra > encodedContent.Length)
							{
								throw new ArgumentOutOfRangeException("offsetToInsertExtra", offsetToInsertExtra,
									"Value must be greater than or equal to 0 and less than or equal to the length (" + encodedContent.Length +
									") of the encoded content of verse " + currVerse);
							}
							if (extra == null)
								throw new ArgumentNullException("extra");
							encodedContent = HttpUtility.HtmlEncode(text.Content.Insert(offsetToInsertExtra, kAwooga)).Replace(kAwooga, extra);
						}
						var content = String.Format("<div id=\"{0}\" class=\"scripttext\">{1}</div>", currVerse,
							encodedContent);
						bldr.Append(content);
					}
				}
			}

			return bldr.ToString();
		}

		public override string ToString()
		{
			return string.IsNullOrEmpty(CharacterId) ? GetText(true) : string.Format("{0}: {1}", CharacterId, GetText(true));
		}

		/// <summary>
		/// Gets whether this block is a quote. It's not 100% reliable since there's the (slight) possibility that the user
		/// could assign the character for a block and then assign it back to Narrator. This would result in UserConfrimed
		/// being set to true even though it was a "non-quote" (unmarked) narrator block. Depending on how this
		/// property gets used in the future, we might need to actually store an additional piece of information about
		/// the block to distinguish this case and prevent a false positive. (For the current planned usage, an occasional
		/// false positive will not be a big deal.)
		/// </summary>
		public bool IsQuote
		{
			get { return !CharacterVerseData.IsCharacterStandard(CharacterId) || UserConfirmed; }
		}

		public bool CharacterIs(string bookId, CharacterVerseData.StandardCharacter standardCharacterType)
		{
			return CharacterId == CharacterVerseData.GetStandardCharacterId(bookId, standardCharacterType);
		}

		public bool CharacterIsUnclear()
		{
			return CharacterId == CharacterVerseData.UnknownCharacter || CharacterId == CharacterVerseData.AmbiguousCharacter;
		}

		public void SetStandardCharacter(string bookId, CharacterVerseData.StandardCharacter standardCharacterType)
		{
			CharacterId = CharacterVerseData.GetStandardCharacterId(bookId, standardCharacterType);
			Delivery = null;
		}

		public string GetAsXml(bool includeXmlDeclaration = true)
		{
			return XmlSerializationHelper.SerializeToString(this, !includeXmlDeclaration);
		}

		public void SetCharacterAndDelivery(IEnumerable<CharacterVerse> characters)
		{
			var characterList = characters.ToList();
			if (characterList.Count == 1)
			{
				SetCharacterAndCharacterIdInScript(characterList[0].Character, () => characterList[0]);
				Delivery = characterList[0].Delivery;
			}
			else if (characterList.Count == 0)
			{
				CharacterId = CharacterVerseData.UnknownCharacter;
				Delivery = null;
			}
			else
			{
				// Might all represent the same Character/Delivery. Need to check.
				var set = new SortedSet<CharacterVerse>(characterList, new CharacterDeliveryComparer());
				if (set.Count == 1)
				{
					SetCharacterAndCharacterIdInScript(characterList[0].Character, () => characterList[0]);
					Delivery = set.First().Delivery;
				}
				else
				{
					CharacterId = CharacterVerseData.AmbiguousCharacter;
					Delivery = null;
				}
			}
		}

		public void SetCharacterAndCharacterIdInScript(string characterId, int bookNumber, Paratext.ScrVers scrVers = null)
		{
			SetCharacterAndCharacterIdInScript(characterId, () => GetMatchingCharacter(bookNumber, scrVers));
		}

		private void SetCharacterAndCharacterIdInScript(string characterId, Func<CharacterVerse> getMatchingCharacterForVerse)
		{
			if (CharacterId == characterId && CharacterIdOverrideForScript != null)
				return;
			CharacterId = characterId;
			UseDefaultForMultipleChoiceCharacter(getMatchingCharacterForVerse);
		}

		public void UseDefaultForMultipleChoiceCharacter(int bookNumber, Paratext.ScrVers scrVers = null)
		{
			UseDefaultForMultipleChoiceCharacter(() => GetMatchingCharacter(bookNumber, scrVers));
		}

		public void UseDefaultForMultipleChoiceCharacter(Func<CharacterVerse> getMatchingCharacterForVerse)
		{
			var ids = CharacterId.SplitCharacterId(2);
			if (ids.Length > 1)
			{
				var cv = getMatchingCharacterForVerse();
				CharacterIdInScript = (cv != null && !String.IsNullOrEmpty(cv.DefaultCharacter) ? cv.DefaultCharacter : ids[0]);
			}
		}

		private CharacterVerse GetMatchingCharacter(int bookNumber, Paratext.ScrVers scrVers)
		{
			return GetMatchingCharacter(ControlCharacterVerseData.Singleton, bookNumber, scrVers);
		}

		public CharacterVerse GetMatchingCharacter(ICharacterVerseInfo cvInfo, int bookNumber, Paratext.ScrVers scrVers)
		{
			return cvInfo.GetCharacters(bookNumber, ChapterNumber, InitialStartVerseNumber,
				InitialEndVerseNumber, versification: scrVers).FirstOrDefault(c => c.Character == CharacterId);
		}
	}

	public class BlockComparer : IEqualityComparer<Block>
	{
		public bool Equals(Block x, Block y)
		{
			if (x == null && y == null)
				return true;
			if (x == null || y == null)
				return false;

			return x.StyleTag == y.StyleTag &&
				x.IsParagraphStart == y.IsParagraphStart &&
				x.ChapterNumber == y.ChapterNumber &&
				x.InitialStartVerseNumber == y.InitialStartVerseNumber &&
				x.InitialEndVerseNumber == y.InitialEndVerseNumber &&
				x.CharacterId == y.CharacterId &&
				x.CharacterIdOverrideForScript == y.CharacterIdOverrideForScript &&
				x.Delivery == y.Delivery &&
				x.UserConfirmed == y.UserConfirmed &&
				x.MultiBlockQuote == y.MultiBlockQuote &&
				x.SplitId == y.SplitId &&
				x.BlockElements.SequenceEqual(y.BlockElements, new BlockElementContentsComparer());
		}

		public int GetHashCode(Block obj)
		{
			return obj.GetHashCode();
		}
	}

	public class SplitBlockComparer : IEqualityComparer<Block>
	{
		public bool Equals(Block x, Block y)
		{
			if (x == null && y == null)
				return true;
			if (x == null || y == null)
				return false;

			return x.ChapterNumber == y.ChapterNumber &&
				x.InitialStartVerseNumber == y.InitialStartVerseNumber &&
				x.InitialEndVerseNumber == y.InitialEndVerseNumber &&
				x.BlockElements.SequenceEqual(y.BlockElements, new BlockElementContentsComparer());
		}

		public int GetHashCode(Block obj)
		{
			return obj.GetHashCode();
		}
	}
}
