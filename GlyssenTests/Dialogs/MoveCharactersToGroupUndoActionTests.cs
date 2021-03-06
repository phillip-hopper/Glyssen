﻿using System.Collections.Generic;
using System.Linq;
using Glyssen;
using Glyssen.Character;
using Glyssen.Dialogs;
using NUnit.Framework;
using SIL.Extensions;

namespace GlyssenTests.Dialogs
{
	[TestFixture]
	internal class MoveCharactersToGroupUndoActionTests
	{
		private Project m_testProject;
		private IComparer<string> m_priorityComparer;

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			m_testProject = TestProject.CreateTestProject(TestProject.TestBook.MRK);
			m_priorityComparer = new CharacterByKeyStrokeComparer(m_testProject.GetKeyStrokesByCharacterId());
		}

		[SetUp]
		public void SetUp()
		{
			m_testProject.CharacterGroupList.CharacterGroups.Clear();
		}

		private CharacterGroup AddCharacterGroup(params string[] characterIds)
		{
			var group = new CharacterGroup(m_testProject, m_priorityComparer);
			foreach (var character in characterIds)
				group.CharacterIds.Add(character);
			m_testProject.CharacterGroupList.CharacterGroups.Add(group);
			return group;
		}

		[Test]
		public void Constructor_SourceAndDestHaveCharactersBeforeAndAfterMove_CharactersGetMovedNoGroupsDeleted()
		{
			var sourceGroup = AddCharacterGroup("Paul", "Jacob", "Micah");
			var destGroup = AddCharacterGroup("centurion", "man, another", "captain", "Pharisees");

			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, destGroup, new List<string> { "Micah" });

			Assert.IsTrue(action.GroupsAffectedByLastOperation.SequenceEqual(new[] { destGroup, sourceGroup }));
			Assert.IsTrue(sourceGroup.CharacterIds.SetEquals(new[] { "Paul", "Jacob" }));
			Assert.IsTrue(destGroup.CharacterIds.SetEquals(new[] { "centurion", "man, another", "captain", "Pharisees", "Micah" }));
		}

		[Test]
		public void Constructor_AllCharactersMovedFromSourceWithNoAssignedActor_CharactersGetMovedAndSourceIsDeleted()
		{
			var charactersToMove = new[] { "Paul", "Jacob", "Micah" };
			var sourceGroup = AddCharacterGroup(charactersToMove);
			var destGroup = AddCharacterGroup("centurion", "man, another", "captain", "Pharisees");

			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, destGroup, charactersToMove);

			Assert.AreEqual(destGroup, action.GroupsAffectedByLastOperation.Single());
			m_testProject.CharacterGroupList.CharacterGroups.SetEquals(new[] {destGroup});
			Assert.IsTrue(destGroup.CharacterIds.SetEquals(new[] { "centurion", "man, another", "captain", "Pharisees", "Paul", "Jacob", "Micah" }));
		}

		[Test]
		public void Constructor_NoDestSupplied_CharactersGetMovedToNewGroup()
		{
			var sourceGroup = AddCharacterGroup("Paul", "Jacob", "Micah", "centurion", "man, another");
			var anotherGroup = AddCharacterGroup("captain", "Pharisees");
			var charactersToMove = new List<string> { "Micah", "man, another" };
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, null, charactersToMove);

			var newGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("Micah");
			Assert.IsTrue(action.GroupsAffectedByLastOperation.SequenceEqual(new[] { newGroup, sourceGroup }));
			Assert.AreNotEqual(sourceGroup, newGroup);
			Assert.IsTrue(newGroup.CharacterIds.SetEquals(charactersToMove));
			Assert.IsTrue(sourceGroup.CharacterIds.SetEquals(new[] { "Paul", "Jacob", "centurion" }));
			Assert.IsTrue(anotherGroup.CharacterIds.SetEquals(new[] { "captain", "Pharisees" }));
		}

		[Test]
		public void Constructor_NoDestSuppliedMoveAllCharactersFromSourceWithAssignedActor_CharactersGetMovedToNewGroupAndSourceLeftEmpty()
		{
			var charactersToMove = new [] { "Paul", "Jacob", "Micah", "centurion", "man, another" };
			var sourceGroup = AddCharacterGroup(charactersToMove);
			sourceGroup.AssignVoiceActor(13);
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, null, new List<string>(sourceGroup.CharacterIds));

			var newGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("Micah");
			Assert.IsTrue(action.GroupsAffectedByLastOperation.SequenceEqual(new[] { newGroup, sourceGroup }));
			Assert.AreNotEqual(sourceGroup, newGroup);
			Assert.IsTrue(newGroup.CharacterIds.SetEquals(charactersToMove));
			Assert.AreEqual(0, sourceGroup.CharacterIds.Count);
			Assert.AreEqual(13, sourceGroup.VoiceActorId);
		}

		[Test]
		public void Description_MoveToGroupWithImplicitName_GroupReferencedByMajorCharacter()
		{
			var sourceGroup = AddCharacterGroup("Paul", "Jacob", "Micah");
			var destGroup = AddCharacterGroup("centurion", "man, another", "captain", "Pharisees");

			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, destGroup, new List<string> { "Micah" });
			Assert.AreEqual("Move characters to Pharisees group", action.Description);
		}

		[Test]
		public void Description_MoveToGroupWithExplicitName_GroupReferencedByName()
		{
			var sourceGroup = AddCharacterGroup("Paul", "Jacob", "Micah");
			var destGroup = AddCharacterGroup("centurion", "man, another", "captain", "Pharisees");
			destGroup.GroupNumber = 43;
			destGroup.Name = "Forty-three";

			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, destGroup, new List<string> { "Micah" });
			Assert.AreEqual("Move characters to Forty-three group", action.Description);
		}

		[Test]
		public void Description_NoDestinationGroupSpecified_CreateNewGroup()
		{
			var sourceGroup = AddCharacterGroup("Paul", "Jacob", "Micah");

			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, null, new List<string> { "Micah" });
			Assert.AreEqual("Create new group", action.Description);
		}

		[Test]
		public void Description_IsSplit_SplitGroup()
		{
			var sourceGroup = AddCharacterGroup("Paul", "Jacob", "Micah");

			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, null, new List<string> { "Micah" });
			action.IsSplit = true;
			Assert.AreEqual("Split group", action.Description);
		}

		[Test]
		public void Undo_SourceAndDestHaveCharactersBeforeAndAfterMove_CharactersGetMovedBackToOriginalGroup()
		{
			var originalCharactersInSource = new[] { "Paul", "Jacob", "Micah", "ear", "centurion", "man, another", "captain", "Pharisees", "Rhoda" };
			var originalCharactersInDest = new[] { "children", "Martha" };
			var sourceGroup = AddCharacterGroup(originalCharactersInSource);
			var destGroup = AddCharacterGroup(originalCharactersInDest);
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, destGroup, new List<string> { "Rhoda", "ear" });

			Assert.IsTrue(action.Undo());
			Assert.IsTrue(sourceGroup.CharacterIds.SetEquals(originalCharactersInSource));
			Assert.IsTrue(action.GroupsAffectedByLastOperation.SequenceEqual(new [] { sourceGroup, destGroup }));
			Assert.AreEqual(2, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.IsTrue(sourceGroup.CharacterIds.SetEquals(originalCharactersInSource));
			Assert.IsTrue(destGroup.CharacterIds.SetEquals(originalCharactersInDest));
		}

		[Test]
		public void Undo_AllCharactersMovedFromSourceWithNoAssignedActor_GroupRecreatedAndCharactersGetMovedBack()
		{
			var charactersToMove = new[] { "Paul", "Jacob", "Micah" };
			var originalCharactersInDest = new[] { "centurion", "man, another", "captain", "Pharisees" };
			var sourceGroup = AddCharacterGroup(charactersToMove);
			var destGroup = AddCharacterGroup(originalCharactersInDest);
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, destGroup, charactersToMove);
			Assert.AreEqual(1, m_testProject.CharacterGroupList.CharacterGroups.Count);

			Assert.IsTrue(action.Undo());
			var recreatedGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("Micah");
			Assert.IsTrue(recreatedGroup.CharacterIds.SetEquals(charactersToMove));
			Assert.IsTrue(action.GroupsAffectedByLastOperation.SequenceEqual(new[] { recreatedGroup, destGroup }));
			Assert.AreEqual(2, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.IsTrue(recreatedGroup.CharacterIds.SetEquals(charactersToMove));
			Assert.IsTrue(destGroup.CharacterIds.SetEquals(originalCharactersInDest));
		}

		[Test]
		public void Undo_NoDestSuppliedMoveAllCharactersFromSourceWithAssignedActor_CharactersGetMovedBackToOriginalGroupAndNewGroupIsRemoved()
		{
			var originalCharactersInSource = new[] { "Paul", "Jacob", "Micah", "centurion", "man, another", "captain", "Pharisees" };
			var sourceGroup = AddCharacterGroup(originalCharactersInSource);
			sourceGroup.AssignVoiceActor(13);
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, null, new List<string>(sourceGroup.CharacterIds));

			Assert.IsTrue(action.Undo());
			Assert.IsTrue(sourceGroup.CharacterIds.SetEquals(originalCharactersInSource));
			Assert.AreEqual(sourceGroup, action.GroupsAffectedByLastOperation.Single());
			Assert.AreEqual(1, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.AreEqual(sourceGroup, m_testProject.CharacterGroupList.GroupContainingCharacterId("Micah"));
		}

		[Test]
		public void Undo_Split_CharactersGetMovedBackToOriginalGroupAndNewGroupIsRemoved()
		{
			var originalCharactersInSource = new[] { "Paul", "Jacob", "Micah", "centurion", "man, another", "captain", "Pharisees" };
			var sourceGroup = AddCharacterGroup(originalCharactersInSource);
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, null, new List<string> { "Micah", "man, another", "captain" });
			action.IsSplit = true;

			Assert.IsTrue(action.Undo());
			Assert.IsTrue(sourceGroup.CharacterIds.SetEquals(originalCharactersInSource));
			Assert.AreEqual(sourceGroup, action.GroupsAffectedByLastOperation.Single());
			Assert.AreEqual(1, m_testProject.CharacterGroupList.CharacterGroups.Count);
			Assert.AreEqual(sourceGroup, m_testProject.CharacterGroupList.GroupContainingCharacterId("Micah"));
		}

		[Test]
		public void Redo_SourceAndDestHaveCharactersBeforeAndAfterMove_CharactersGetMovedToDestNoGroupsDeleted()
		{
			var originalCharactersInSource = new[] { "Paul", "Jacob", "Micah", "ear", "centurion", "man, another", "captain", "Pharisees", "Rhoda" };
			var originalCharactersInDest = new[] { "children", "Martha" };
			var sourceGroup = AddCharacterGroup(originalCharactersInSource);
			var destGroup = AddCharacterGroup(originalCharactersInDest);
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, destGroup, new List<string> { "Rhoda", "ear" });

			Assert.IsTrue(action.Undo());
			Assert.IsTrue(action.Redo());

			Assert.IsTrue(action.GroupsAffectedByLastOperation.SequenceEqual(new[] { destGroup, sourceGroup }));
			Assert.IsTrue(sourceGroup.CharacterIds.SetEquals(new[] { "Paul", "Jacob", "Micah", "centurion", "man, another", "captain", "Pharisees" }));
			Assert.IsTrue(destGroup.CharacterIds.SetEquals(new[] { "children", "Martha", "Rhoda", "ear" }));
			Assert.AreEqual(2, m_testProject.CharacterGroupList.CharacterGroups.Count);
		}

		[Test]
		public void Redo_AllCharactersMovedFromSourceWithNoAssignedActor_GroupRecreatedAndCharactersGetMovedBack()
		{
			var charactersToMove = new[] { "Paul", "Jacob", "Micah" };
			var originalCharactersInDest = new[] { "centurion", "man, another", "captain", "Pharisees" };
			var sourceGroup = AddCharacterGroup(charactersToMove);
			var destGroup = AddCharacterGroup(originalCharactersInDest);
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, destGroup, charactersToMove);
			Assert.AreEqual(1, m_testProject.CharacterGroupList.CharacterGroups.Count);

			Assert.IsTrue(action.Undo());
			Assert.IsTrue(action.Redo());

			Assert.AreEqual(destGroup, action.GroupsAffectedByLastOperation.Single());
			m_testProject.CharacterGroupList.CharacterGroups.SetEquals(new[] { destGroup });
			Assert.IsTrue(destGroup.CharacterIds.SetEquals(new[] { "centurion", "man, another", "captain", "Pharisees", "Paul", "Jacob", "Micah" }));
		}

		[Test]
		public void Redo_NoDestSuppliedMoveAllCharactersFromSourceWithAssignedActor_CharactersGetMovedBackToOriginalGroupAndNewGroupIsRemoved()
		{
			var charactersToMove = new[] { "Paul", "Jacob", "Micah", "centurion", "man, another", "captain", "Pharisees" };
			var sourceGroup = AddCharacterGroup(charactersToMove);
			sourceGroup.AssignVoiceActor(13);
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, null, new List<string>(sourceGroup.CharacterIds));

			Assert.IsTrue(action.Undo());
			Assert.IsTrue(action.Redo());

			var newGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("Micah");
			Assert.IsTrue(action.GroupsAffectedByLastOperation.SequenceEqual(new[] { newGroup, sourceGroup }));
			Assert.AreNotEqual(sourceGroup, newGroup);
			Assert.IsTrue(newGroup.CharacterIds.SetEquals(charactersToMove));
			Assert.AreEqual(0, sourceGroup.CharacterIds.Count);
			Assert.AreEqual(13, sourceGroup.VoiceActorId);
		}

		[Test]
		public void Redo_Split_CharactersGetMovedBackToOriginalGroupAndNewGroupIsRemoved()
		{
			var originalCharactersInSource = new[] { "Paul", "Jacob", "Micah", "centurion", "man, another", "captain", "Pharisees" };
			var charactersToMove = new List<string> { "Micah", "man, another" };
			var sourceGroup = AddCharacterGroup(originalCharactersInSource);
			var action = new MoveCharactersToGroupUndoAction(m_testProject, sourceGroup, null, new List<string>(charactersToMove));
			action.IsSplit = true;

			Assert.IsTrue(action.Undo());
			Assert.IsTrue(action.Redo());

			var newGroup = m_testProject.CharacterGroupList.GroupContainingCharacterId("Micah");
			Assert.IsTrue(action.GroupsAffectedByLastOperation.SequenceEqual(new[] { newGroup, sourceGroup }));
			Assert.AreNotEqual(sourceGroup, newGroup);
			Assert.IsTrue(newGroup.CharacterIds.SetEquals(charactersToMove));
			Assert.IsTrue(sourceGroup.CharacterIds.SetEquals(new[] { "Paul", "Jacob", "centurion", "captain", "Pharisees" }));
		}
	}
}