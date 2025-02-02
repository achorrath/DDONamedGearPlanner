﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace DDONamedGearPlanner
{
	public enum EquipmentSlotType
	{
		None,
		Back,
		Body,
		Eye,
		Feet,
		Finger1,
		Finger2,
		Hand,
		Head,
		Neck,
		Offhand,
		Trinket,
		Waist,
		Weapon,
		Wrist
	}

	[Flags]
	public enum SlotType
	{
		None = 0,
		Back = 1,
		Body = 2,
		Eye = 4,
		Feet = 8,
		Finger = 16,
		Hand = 32,
		Head = 64,
		Neck = 128,
		Offhand = 256,
		Trinket = 512,
		Waist = 1024,
		Weapon = 2048,
		Wrist = 4096
	}

	public enum ItemDataSource { None, Dataset, Custom, Cannith, SlaveLord, ThunderForge, LegendaryGreenSteel }


	static class EquipmentSlotTypeConversionExtensions
	{
		public static SlotType ToSlotType(this EquipmentSlotType est)
		{
			if (est == EquipmentSlotType.Finger1 || est == EquipmentSlotType.Finger2) return SlotType.Finger;
			else return (SlotType)Enum.Parse(typeof(SlotType), est.ToString());
		}

		public static EquipmentSlotType ToEquipmentSlotType(this SlotType st)
		{
			if (st == SlotType.Finger) return EquipmentSlotType.None;
			else return (EquipmentSlotType)Enum.Parse(typeof(EquipmentSlotType), st.ToString());
		}
	}

	[Flags]
	public enum ArmorCategory
	{
		Cloth = 1,
		Light = 2,
		Medium = 4,
		Heavy = 8,
		Docent = 16
	}

	[Flags]
	public enum OffhandCategory
	{
		Buckler = 1,
		Small = 2,
		Large = 4,
		Tower = 8,
		Orb = 16,
		RuneArm = 32
	}

	[Flags]
	public enum WeaponCategory
	{
		Simple = 1,
		Martial = 2,
		Exotic = 4,
		Throwing = 8
	}

	[Serializable]
	public class ItemProperty
	{
		public string Property { get; set; }
		public string Type { get; set; }
		public float Value { get; set; }
		public List<ItemProperty> Options;
		public bool HideOptions;
		public DDOItemData Owner;
		// this will only ever be used by the interface to create ad hoc item properties in order to track set bonuses in gear sets
		public string SetBonusOwner;

		public ItemProperty Duplicate()
		{
			ItemProperty ip = new ItemProperty
			{
				Property = Property,
				Type = Type,
				Value = Value
			};
			if (Options != null)
			{
				ip.Options = new List<ItemProperty>();
				foreach (var o in Options)
				{
					ItemProperty op = o.Duplicate();
					ip.Options.Add(op);
				}
			}

			return ip;
		}

		public override string ToString()
		{
			return "{" + (string.IsNullOrWhiteSpace(Type) ? "untyped" : Type) + " " + Property + " " + Value + "}";
		}
	}

	[Serializable]
	public class DDOAdventurePackData
	{
		public string Name;
		public List<DDOQuestData> Quests = new List<DDOQuestData>();
		public bool FreeToVIP;
	}

	[Serializable]
	public class DDOQuestData
	{
		public string Name;
		public DDOAdventurePackData Adpack;
		public bool IsRaid;
		public bool IsFree;
		public List<DDOItemData> Items = new List<DDOItemData>();
	}

	[Serializable]
	public class DDOItemData
	{
		public string Name { get; set; }
		public string WikiURL;
		public SlotType Slot { get; set; }
		public int Category;
		public List<ItemProperty> Properties = new List<ItemProperty>();
		public readonly bool MinorArtifact;
		public readonly ItemDataSource Source;
		public DDOQuestData QuestFoundIn;

		string _IconName;
		public string IconName
		{
			get
			{
				if (_IconName == null)
				{
					_IconName = Name.Replace("?", "").Replace('-', ' ').Replace('.', ' ').Replace(":", "");

					int c = _IconName.IndexOf("(tier");
					if (c > -1) _IconName = _IconName.Substring(0, c);

					c = _IconName.IndexOf("(Level");
					if (c == -1) c = _IconName.IndexOf("(level");
					if (c > -1) _IconName = _IconName.Substring(0, c);

					c = _IconName.IndexOf("(unsuppressed)");
					if (c > -1) _IconName = _IconName.Substring(0, c);

					c = _IconName.IndexOf(" of the Weapon Master");
					if (c > -1) _IconName = _IconName.Substring(0, c);

					if (_IconName.StartsWith("Fellblade (")) _IconName = "Fellblade";
					else if (_IconName.StartsWith("Mithral Full Plate of Speed")) _IconName = "Mithral Full Plate of Speed";
					//else if (_IconName == "Ir'Kesslan's Most Prescient Lens") _IconName = "ir'Kesslan's Most Prescient Lens";
					//else if (_IconName == "Linen Handwraps") _IconName = "Linen Wraps";
					else if (_IconName.StartsWith("The Arc Welder (")) _IconName = "The Arc Welder";
					else if (_IconName.StartsWith("The Legendary Arc Welder (")) _IconName = "The Legendary Arc Welder";
					else if (_IconName.StartsWith("Thought Spike (")) _IconName = "Thought Spike";

					_IconName = _IconName.Trim();

					return _IconName;
				}
				else return _IconName;
			}

			set { _IconName = value; }
		}

		// utility because it gets used so often
		int _Handedness = -1;
		public int Handedness
		{
			get
			{
				if (_Handedness > -1) return _Handedness;
				if (Slot != SlotType.Weapon) _Handedness = 0;
				else _Handedness = (int)Properties.Find(p => p.Property == "Handedness").Value;
				return _Handedness;
			}
		}

		// utility because it gets used so often
		string _WeaponType;
		public string WeaponType
		{
			get
			{
				if (_WeaponType != null) return _WeaponType;
				if (Slot != SlotType.Weapon) return "";
				else _WeaponType = Properties.Find(p => p.Property == "Weapon Type").Type;
				return _WeaponType;
			}
		}

		//utility because it gets used so often
		int _ML = -1;
		public int ML
		{
			get
			{
				if (_ML != -1) return _ML;
				_ML = (int)(Properties.Find(p => p.Property == "Minimum Level")?.Value ?? 1);
				return _ML;
			}
		}

		public DDOItemData(ItemDataSource source, bool ma)
		{
			Source = source;
			MinorArtifact = ma;
		}

		public ItemProperty AddProperty(string prop, string type, float value, List<ItemProperty> options, int insertat = -1)
		{
			ItemProperty ip = new ItemProperty { Property = prop, Type = type, Value = value, Options = options, Owner = this };
			if (type == "insightful") ip.Type = "insight";
			if (options != null)
				foreach (var i in options) i.Owner = this;
			if (insertat == -1) Properties.Add(ip);
			else Properties.Insert(insertat, ip);
			return ip;
		}

		public override string ToString()
		{
			return Name;
		}

		public DDOItemData Duplicate()
		{
			DDOItemData item = new DDOItemData(Source, MinorArtifact)
			{
				Name = Name,
				WikiURL = WikiURL,
				Slot = Slot,
				Category = Category,
				QuestFoundIn = QuestFoundIn
			};
			foreach (var p in Properties)
			{
				ItemProperty ip = p.Duplicate();
				ip.Owner = item;
				if (ip.Options != null)
				{
					foreach (ItemProperty ipt in ip.Options)
						ipt.Owner = item;
				}
				item.Properties.Add(ip);
			}
			QuestFoundIn?.Items.Add(item);

			return item;
		}

		// this is used only by the saving of custom items
		// if this becomes the official way to save item data, it will need adjusting
		public XmlElement ToXml(XmlDocument doc)
		{
			XmlElement xi = doc.CreateElement("Item");
			XmlAttribute xa = doc.CreateAttribute("source");
			xa.InnerText = Source.ToString();
			xi.Attributes.Append(xa);
			XmlElement xe = doc.CreateElement("Name");
			xe.InnerText = Name;
			xi.AppendChild(xe);
			// we skip WikiURL and MinorArtifact for custom items
			xe = doc.CreateElement("Slot");
			xe.InnerText = Slot.ToString();
			xi.AppendChild(xe);
			xe = doc.CreateElement("Category");
			xe.InnerText = Category.ToString();
			xi.AppendChild(xe);
			xe = doc.CreateElement("Properties");
			xi.AppendChild(xe);
			foreach (var p in Properties)
			{
				XmlElement xp = doc.CreateElement("Property");
				xp.InnerText = p.Property;
				xa = doc.CreateAttribute("type");
				xa.InnerText = string.IsNullOrWhiteSpace(p.Type) ? "untyped" : p.Type;
				xp.Attributes.Append(xa);
				xa = doc.CreateAttribute("value");
				xa.InnerText = p.Value.ToString();
				xp.Attributes.Append(xa);
				xe.AppendChild(xp);
			}

			return xi;
		}

		public static DDOItemData FromXml(XmlElement xe)
		{
			try
			{
				DDOItemData item = new DDOItemData((ItemDataSource)Enum.Parse(typeof(ItemDataSource), xe.GetAttribute("source")), false);

				item.Name = xe.GetElementsByTagName("Name")[0].InnerText;
				item.Slot = (SlotType)Enum.Parse(typeof(SlotType), xe.GetElementsByTagName("Slot")[0].InnerText);
				item.Category = int.Parse(xe.GetElementsByTagName("Category")[0].InnerText);
				foreach (XmlElement xp in xe.GetElementsByTagName("Property"))
				{
					string type = xp.GetAttribute("type");
					if (type == "untyped") type = null;
					item.AddProperty(xp.InnerText, type, float.Parse(xp.GetAttribute("value")), null);
				}

				return item;
			}
			catch
			{
				return null;
			}
		}
	}

	[Serializable]
	public class DDOSlot
	{
		public SlotType Slot;
		public List<DDOItemData> Items = new List<DDOItemData>();
		public Type CategoryEnumType;
	}

	[Serializable]
	public class DDOItemProperty
	{
		public string Property { get; set; }
		public List<string> Types = new List<string>();
		public List<DDOItemData> Items = new List<DDOItemData>();
		public SlotType SlotsFoundOn;
	}

	[Serializable]
	public class DDOItemSetBonusProperty
	{
		public string Property;
		public string Type;
		public float Value;
	}

	[Serializable]
	public class DDOItemSetBonus
	{
		public int MinimumItems;
		public List<DDOItemSetBonusProperty> Bonuses = new List<DDOItemSetBonusProperty>();
	}

	[Serializable]
	public class DDOItemSet
	{
		public string Name;
		public string WikiURL;
		public List<DDOItemSetBonus> SetBonuses;
		public List<DDOItemData> Items = new List<DDOItemData>();

        public virtual DDOItemSetBonus GetSetBonuses(List<ItemProperty> itemprops)
        {
            DDOItemSetBonus sb = null;
            foreach (var SB in SetBonuses)
            {
                if (SB.MinimumItems > itemprops.Count) break;
                sb = SB;
            }

            return sb;
        }
	}

	[Serializable]
    public class LGSItemSet : DDOItemSet
    {
		public LGSItemSet()
        {
            Name = "Legendary Green Steel";
            WikiURL = null;
        }

        public override DDOItemSetBonus GetSetBonuses(List<ItemProperty> itemprops)
        {
            // need at least 2 items for any LGS set bonuses
            if (itemprops.Count < 2) return null;

            DDOItemSetBonus sb = new DDOItemSetBonus();

			Dictionary<string, List<string>> gems = new Dictionary<string, List<string>>();
			gems["Dominion"] = new List<string>();
			gems["Escalation"] = new List<string>();
			gems["Opposition"] = new List<string>();
			gems["Ethereal"] = new List<string>();
			gems["Material"] = new List<string>();

			foreach (var ip in itemprops)
				foreach (var op in ip.Options)
				{
					string type = null;
					if (op.Property.EndsWith("Dominion")) type = "Dominion";
					else if (op.Property.EndsWith("Escalation")) type = "Escalation";
					else if (op.Property.EndsWith("Opposition")) type = "Opposition";
					else if (op.Property.EndsWith("Ethereal Essence")) type = "Ethereal";
					else if (op.Property.EndsWith("Material Essence")) type = "Material";

					if (type != null) gems[type].Add(op.Type);
				}

			// determine which gem counts are highest
			string hg = gems["Escalation"].Count > gems["Dominion"].Count ? "Escalation" : "Dominion";
			if (gems["Opposition"].Count > gems[hg].Count) hg = gems["Dominion"].Count == gems["Escalation"].Count ? "Opposition" : null;

			// we have a dominant gem to apply a bonus for
			if (hg != null)
			{
				int unique = gems[hg].Distinct().Count();
				if (hg == "Dominion") sb.Bonuses.Add(new DDOItemSetBonusProperty { Property = "Maximum Spell Points %", Type = "legendary", Value = 6 + unique * 2 });
				else if (hg == "Escalation") sb.Bonuses.Add(new DDOItemSetBonusProperty { Property = "Dodge Cap", Type = "legendary", Value = 1 + unique / 3 });
				else if (hg == "Opposition") sb.Bonuses.Add(new DDOItemSetBonusProperty { Property = "Maximum Hit Points %", Type = "legendary", Value = 6 + unique * 2 });
			}

			// having 4+pcs means potentially an ethereal or material essence bonus
			if (itemprops.Count >= 4)
			{
				string he = gems["Ethereal"].Count > gems["Material"].Count ? "Ethereal" : null;
				if (he == null) he = gems["Material"].Count > gems["Ethereal"].Count ? "Material" : null;

				if (he != null)
				{
					int unique = gems[he].Distinct().Count();
					if (he == "Ethereal") sb.Bonuses.Add(new DDOItemSetBonusProperty { Property = "Incorporeal", Type = "legendary", Value = 1 + unique * 0.5f });
					else if (he == "Material")
					{
						sb.Bonuses.Add(new DDOItemSetBonusProperty { Property = "Critical Hit Damage", Type = "legendary", Value = 1 + unique });
						sb.Bonuses.Add(new DDOItemSetBonusProperty { Property = "Universal Spell Crit Damage", Type = "legendary", Value = 5 + unique * 2 });
					}

					// the 5+pc bonus only applies if the set is also applying gem and essence bonuses
					if (itemprops.Count >= 5 && hg != null)
					{
						if (he == "Ethereal")
						{
							if (hg == "Dominion") sb.Bonuses.Add(new DDOItemSetBonusProperty { Property = "Mind Spike" });
							else if (hg == "Escalation") sb.Bonuses.Add(new DDOItemSetBonusProperty { Property = "Whispers of Life" });
							if (hg == "Opposition") sb.Bonuses.Add(new DDOItemSetBonusProperty { Property = "Brazen Brilliance" });
						}
						else if (he == "Material")
						{
							if (hg == "Dominion") sb.Bonuses.Add(new DDOItemSetBonusProperty { Property = "Controller's Grip" });
							else if (hg == "Escalation") sb.Bonuses.Add(new DDOItemSetBonusProperty { Property = "Sound and Silence" });
							if (hg == "Opposition") sb.Bonuses.Add(new DDOItemSetBonusProperty { Property = "Ender" });
						}
					}
				}
			}

            return sb;
        }
    }

	[Serializable]
	public class DDODataset
	{
		public Dictionary<SlotType, DDOSlot> Slots = new Dictionary<SlotType, DDOSlot>();
		public Dictionary<string, DDOItemProperty> ItemProperties = new Dictionary<string, DDOItemProperty>();
		public List<DDOItemData> Items = new List<DDOItemData>();
		public Dictionary<string, DDOItemSet> Sets = new Dictionary<string, DDOItemSet>();
		public Dictionary<SlotType, List<DDOItemProperty>> SlotExclusiveItemProperties = new Dictionary<SlotType, List<DDOItemProperty>>();
		public List<DDOAdventurePackData> AdventurePacks;

		public void Initialize()
		{
			Slots.Add(SlotType.Back, new DDOSlot { Slot = SlotType.Back });
			Slots.Add(SlotType.Body, new DDOSlot { Slot = SlotType.Body, CategoryEnumType = typeof(ArmorCategory) });
			Slots.Add(SlotType.Eye, new DDOSlot { Slot = SlotType.Eye });
			Slots.Add(SlotType.Feet, new DDOSlot { Slot = SlotType.Feet });
			Slots.Add(SlotType.Finger, new DDOSlot { Slot = SlotType.Finger });
			Slots.Add(SlotType.Hand, new DDOSlot { Slot = SlotType.Hand });
			Slots.Add(SlotType.Head, new DDOSlot { Slot = SlotType.Head });
			Slots.Add(SlotType.Neck, new DDOSlot { Slot = SlotType.Neck });
			Slots.Add(SlotType.Offhand, new DDOSlot { Slot = SlotType.Offhand, CategoryEnumType = typeof(OffhandCategory) });
			Slots.Add(SlotType.Trinket, new DDOSlot { Slot = SlotType.Trinket });
			Slots.Add(SlotType.Waist, new DDOSlot { Slot = SlotType.Waist });
			Slots.Add(SlotType.Weapon, new DDOSlot { Slot = SlotType.Weapon, CategoryEnumType = typeof(WeaponCategory) });
			Slots.Add(SlotType.Wrist, new DDOSlot { Slot = SlotType.Wrist });

			// I hate doing this but there isn't a more expedient way to pull the data from the wiki cleanly

			#region Korvos sets
			Sets.Add("Anger's Wrath", new DDOItemSet
			{
				Name = "Anger's Wrath",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Anger.27s_Wrath",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Attack",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Arcane Mind", new DDOItemSet
			{
				Name = "Arcane Mind",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Arcane_Mind",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Power",
								Type = "equipment",
								Value = 24
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Power",
								Type = "equipment",
								Value = 24
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Power",
								Type = "equipment",
								Value = 24
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Power",
								Type = "equipment",
								Value = 24
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Power",
								Type = "equipment",
								Value = 24
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Power",
								Type = "equipment",
								Value = 24
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Power",
								Type = "equipment",
								Value = 24
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell Power",
								Type = "equipment",
								Value = 24
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Power",
								Type = "equipment",
								Value = 24
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Spell Power",
								Type = "equipment",
								Value = 24
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sonic Spell Power",
								Type = "equipment",
								Value = 24
							}
						}
					}
				}
			});

			Sets.Add("Archivist", new DDOItemSet
			{
				Name = "Archivist",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Archivist",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Spell Points",
								Type = "enhancement",
								Value = 20
							}
						}
					}
				}
			});

			Sets.Add("Devoted Heart", new DDOItemSet
			{
				Name = "Devoted Heart",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Devoted_Heart",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Power",
								Type = "equipment",
								Value = 36
							}
						}
					}
				}
			});

			Sets.Add("Nimble Hand", new DDOItemSet
			{
				Name = "Nimble Hand",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Nimble_Hand",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Attack",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Damage",
								Type = "enhancement",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Pathfinders", new DDOItemSet
			{
				Name = "Pathfinders",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Pathfinders",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Attack",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Protector's Heart", new DDOItemSet
			{
				Name = "Protector's Heart",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Protector.27s_Heart",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Fortification",
								Type = "enhancement",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class",
								Type = "insight",
								Value = 1
							}
						}
					}
				}
			});

			Sets.Add("Troubleshooter", new DDOItemSet
			{
				Name = "Troubleshooter",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Troubleshooter",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Fortitude",
								Type = "insight",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Reflex",
								Type = "insight",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Will",
								Type = "insight",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Open Lock",
								Type = "competence",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Disable Device",
								Type = "competence",
								Value = 3
							}
						}
					}
				}
			});
			#endregion

			#region Chronoscope sets
			Sets.Add("Might of the Abishai", new DDOItemSet
			{
				Name = "Might of the Abishai",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Might_of_the_Abishai",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class",
								Type = "artifact natural armor",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Strength",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Dexterity",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Constitution",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Intelligence",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Wisdom",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Charisma",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Abjuration Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Conjuration Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Divination Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Enchantment Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Illusion Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Necromancy Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Transmutation Spell DC",
								Type = "artifact",
								Value = 1
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 5,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class",
								Type = "artifact natural armor",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Strength",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Dexterity",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Constitution",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Intelligence",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Wisdom",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Charisma",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Abjuration Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Conjuration Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Divination Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Enchantment Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Illusion Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Necromancy Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Transmutation Spell DC",
								Type = "artifact",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Epic Might of the Abishai", new DDOItemSet
			{
				Name = "Epic Might of the Abishai",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Epic_Might_of_the_Abishai",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class",
								Type = "artifact natural armor",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Strength",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Dexterity",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Constitution",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Intelligence",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Wisdom",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Charisma",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Abjuration Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Conjuration Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Divination Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Enchantment Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Illusion Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Necromancy Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Transmutation Spell DC",
								Type = "artifact",
								Value = 2
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 5,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class",
								Type = "artifact natural armor",
								Value = 8
							},
							new DDOItemSetBonusProperty
							{
								Property = "Strength",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Dexterity",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Constitution",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Intelligence",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Wisdom",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Charisma",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Abjuration Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Conjuration Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Divination Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Enchantment Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Illusion Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Necromancy Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Transmutation Spell DC",
								Type = "artifact",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Legendary Might of the Abishai", new DDOItemSet
			{
				Name = "Legendary Might of the Abishai",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Might_of_the_Abishai",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class",
								Type = "artifact natural armor",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Strength",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Dexterity",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Constitution",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Intelligence",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Wisdom",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Charisma",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Abjuration Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Conjuration Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Divination Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Enchantment Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Illusion Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Necromancy Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Transmutation Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Healing Amplification",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Healing Amplification",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Healing Amplification",
								Type = "artifact",
								Value = 10
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 5,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class",
								Type = "artifact natural armor",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Strength",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Dexterity",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Constitution",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Intelligence",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Wisdom",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Charisma",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Abjuration Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Conjuration Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Divination Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Enchantment Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Illusion Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Necromancy Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Transmutation Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Healing Amplification",
								Type = "artifact",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Healing Amplification",
								Type = "artifact",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Healing Amplification",
								Type = "artifact",
								Value = 30
							}
						}
					}
				}
			});
			#endregion

			#region Three Barrel Cove sets
			Sets.Add("Corsair's Cunning", new DDOItemSet
			{
				Name = "Corsair's Cunning",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Corsair.27s_Cunning",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Underwater Action"
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Underwater Action"
							},
							new DDOItemSetBonusProperty
							{
								Property = "Feather Falling"
							}
						}
					}
				}
			});
			#endregion

			#region The Red Fens sets
			Sets.Add("Divine Blessing", new DDOItemSet
			{
				Name = "Divine Blessing",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Divine_Blessing",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Power",
								Type = "equipment",
								Value = 55
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Power",
								Type = "equipment",
								Value = 55
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Power",
								Type = "equipment",
								Value = 55
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell Power",
								Type = "equipment",
								Value = 55
							}
						}
					}
				}
			});

			Sets.Add("Epic Divine Blessing", new DDOItemSet
			{
				Name = "Epic Divine Blessing",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Epic_Divine_Blessing",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Critical Chance",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Critical Chance",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Critical Chance",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Power",
								Type = "artifact",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Power",
								Type = "artifact",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Power",
								Type = "artifact",
								Value = 30
							}
						}
					}
				}
			});

			Sets.Add("Legendary Divine Blessing", new DDOItemSet
			{
				Name = "Legendary Divine Blessing",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Divine_Blessing",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Critical Chance",
								Type = "artifact",
								Value = 6
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Critical Chance",
								Type = "artifact",
								Value = 6
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Critical Chance",
								Type = "artifact",
								Value = 6
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Power",
								Type = "artifact",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Power",
								Type = "artifact",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Power",
								Type = "artifact",
								Value = 30
							}
						}
					}
				}
			});

			Sets.Add("Elder's Knowledge", new DDOItemSet
			{
				Name = "Elder's Knowledge",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Elder.27s_Knowledge",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Critical Hit Chance",
								Type = "insight",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Universal Spell Power",
								Type = "alchemical",
								Value = 12
							}
						}
					}
				}
			});

			Sets.Add("Epic Elder's Knowledge", new DDOItemSet
			{
				Name = "Epic Elder's Knowledge",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Epic_Elder.27s_Knowledge",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Critical Chance",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Critical Chance",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Critical Chance",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Critical Chance",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Critical Chance",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Critical Chance",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Critical Chance",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell Critical Chance",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Critical Chance",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Spell Critical Chance",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sonic Spell Critical Chance",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Critical Damage",
								Type = "legendary",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Critical Damage",
								Type = "legendary",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Critical Damage",
								Type = "legendary",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Critical Damage",
								Type = "legendary",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Critical Damage",
								Type = "legendary",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Critical Damage",
								Type = "legendary",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Critical Damage",
								Type = "legendary",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell Critical Damage",
								Type = "legendary",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Critical Damage",
								Type = "legendary",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Spell Critical Damage",
								Type = "legendary",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sonic Spell Critical Damage",
								Type = "legendary",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Legendary Elder's Knowledge", new DDOItemSet
			{
				Name = "Legendary Elder's Knowledge",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Elder.27s_Knowledge",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Critical Chance",
								Type = "artifact",
								Value = 6
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Critical Chance",
								Type = "artifact",
								Value = 6
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Critical Chance",
								Type = "artifact",
								Value = 6
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Critical Chance",
								Type = "artifact",
								Value = 6
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Critical Chance",
								Type = "artifact",
								Value = 6
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Critical Chance",
								Type = "artifact",
								Value = 6
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Critical Chance",
								Type = "artifact",
								Value = 6
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell Critical Chance",
								Type = "artifact",
								Value = 6
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Critical Chance",
								Type = "artifact",
								Value = 6
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Spell Critical Chance",
								Type = "artifact",
								Value = 6
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sonic Spell Critical Chance",
								Type = "artifact",
								Value = 6
							},
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Critical Damage",
								Type = "legendary",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Critical Damage",
								Type = "legendary",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Critical Damage",
								Type = "legendary",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Critical Damage",
								Type = "legendary",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Critical Damage",
								Type = "legendary",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Critical Damage",
								Type = "legendary",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Critical Damage",
								Type = "legendary",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell Critical Damage",
								Type = "legendary",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Critical Damage",
								Type = "legendary",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Spell Critical Damage",
								Type = "legendary",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sonic Spell Critical Damage",
								Type = "legendary",
								Value = 15
							}
						}
					}
				}
			});

			Sets.Add("Marshwalker", new DDOItemSet
			{
				Name = "Marshwalker",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Marshwalker",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Move Speed",
								Type = "enhancement",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Jump",
								Type = "competence",
								Value = 15
							}
						}
					}
				}
			});

			Sets.Add("Epic Marshwalker", new DDOItemSet
			{
				Name = "Epic Marshwalker",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Epic_Marshwalker",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Move Speed",
								Type = "enhancement",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fortitude",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Reflex",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Will",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Jump",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Tumble",
								Type = "artifact",
								Value = 5
							}
						}
					}
				}
			});

			Sets.Add("Legendary Marshwalker", new DDOItemSet
			{
				Name = "Legendary Marshwalker",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Marshwalker",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Move Speed",
								Type = "enhancement",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Dexterity",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fortitude",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Reflex",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Will",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Jump",
								Type = "artifact",
								Value = 7
							},
							new DDOItemSetBonusProperty
							{
								Property = "Tumble",
								Type = "artifact",
								Value = 7
							}
						}
					}
				}
			});

			Sets.Add("Raven's Eye", new DDOItemSet
			{
				Name = "Raven's Eye",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Raven.27s_Eye",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Attack",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Spot",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Search",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Epic Raven's Eye", new DDOItemSet
			{
				Name = "Epic Raven's Eye",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Epic_Raven.27s_Eye",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Attack",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Confirm Critical Hits",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Critical Hit Damage",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Spot",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Search",
								Type = "artifact",
								Value = 5
							}
						}
					}
				}
			});

			Sets.Add("Legendary Raven's Eye", new DDOItemSet
			{
				Name = "Legendary Raven's Eye",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Raven.27s_Eye",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Attack",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Confirm Critical Hits",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Critical Hit Damage",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Attack",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Damage",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Spot",
								Type = "artifact",
								Value = 7
							},
							new DDOItemSetBonusProperty
							{
								Property = "Search",
								Type = "artifact",
								Value = 7
							}
						}
					}
				}
			});

			Sets.Add("Shaman's Fury", new DDOItemSet
			{
				Name = "Shaman's Fury",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Shaman.27s_Fury",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Power",
								Type = "equipment",
								Value = 55
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Power",
								Type = "equipment",
								Value = 55
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Power",
								Type = "equipment",
								Value = 55
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Power",
								Type = "equipment",
								Value = 55
							}
						}
					}
				}
			});

			Sets.Add("Epic Shaman's Fury", new DDOItemSet
			{
				Name = "Epic Shaman's Fury",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Epic_Shaman.27s_Fury",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Critical Chance",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Critical Chance",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Critical Chance",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Critical Chance",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Power",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Power",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Power",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Power",
								Type = "artifact",
								Value = 20
							}
						}
					}
				}
			});

			Sets.Add("Legendary Shaman's Fury", new DDOItemSet
			{
				Name = "Legendary Shaman's Fury",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Shaman.27s_Fury",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Critical Chance",
								Type = "artifact",
								Value = 6
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Critical Chance",
								Type = "artifact",
								Value = 6
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Critical Chance",
								Type = "artifact",
								Value = 6
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Critical Chance",
								Type = "artifact",
								Value = 6
							},
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Power",
								Type = "artifact",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Power",
								Type = "artifact",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Power",
								Type = "artifact",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Power",
								Type = "artifact",
								Value = 30
							}
						}
					}
				}
			});

			Sets.Add("Siren's Ward", new DDOItemSet
			{
				Name = "Siren's Ward",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Siren.27s_Ward",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Fortitude",
								Type = "insight",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Reflex",
								Type = "insight",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Will",
								Type = "insight",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class",
								Type = "insight",
								Value = 6
							}
						}
					}
				}
			});

			Sets.Add("Epic Siren's Ward", new DDOItemSet
			{
				Name = "Epic Siren's Ward",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Epic_Siren.27s_Ward",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class %",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fortitude",
								Type = "insight",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Reflex",
								Type = "insight",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Will",
								Type = "insight",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class",
								Type = "artifact shield",
								Value = 3
							}
						}
					}
				}
			});

			Sets.Add("Legendary Siren's Ward", new DDOItemSet
			{
				Name = "Legendary Siren's Ward",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Siren.27s_Ward",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class %",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fortitude",
								Type = "insight",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Reflex",
								Type = "insight",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Will",
								Type = "insight",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class",
								Type = "artifact shield",
								Value = 3
							}
						}
					}
				}
			});

			Sets.Add("Vulkoor's Cunning", new DDOItemSet
			{
				Name = "Vulkoor's Cunning",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Vulkoor.27s_Cunning",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Melee Threat Reduction",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Vulkoorim Poison"
							}
						}
					}
				}
			});

			Sets.Add("Epic Vulkoor's Cunning", new DDOItemSet
			{
				Name = "Epic Vulkoor's Cunning",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Epic_Vulkoor.27s_Cunning",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Melee Threat Reduction",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Powerful Vulkoorim Poison"
							}
						}
					}
				}
			});

			Sets.Add("Legendary Vulkoor's Cunning", new DDOItemSet
			{
				Name = "Legendary Vulkoor's Cunning",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Vulkoor.27s_Cunning",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Melee Threat Reduction",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Powerful Vulkoorim Poison"
							}
						}
					}
				}
			});

			Sets.Add("Vulkoor's Might", new DDOItemSet
			{
				Name = "Vulkoor's Might",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Vulkoor.27s_Might",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Attack",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Threat Generation",
								Value = 20
							}
						}
					}
				}
			});

			Sets.Add("Epic Vulkoor's Might", new DDOItemSet
			{
				Name = "Epic Vulkoor's Might",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Epic_Vulkoor.27s_Might",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Attack",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Threat Generation",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Confirm Critical Hits",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Critical Hit Damage",
								Type = "artifact",
								Value = 3
							}
						}
					}
				}
			});

			Sets.Add("Legendary Vulkoor's Might", new DDOItemSet
			{
				Name = "Legendary Vulkoor's Might",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Vulkoor.27s_Might",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Attack",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Threat Generation",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Confirm Critical Hits",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Critical Hit Damage",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Attack",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Damage",
								Type = "artifact",
								Value = 3
							}
						}
					}
				}
			});
			#endregion

			#region Wrath of Sora Kell set
			Sets.Add("Wrath of Sora Kell", new DDOItemSet
			{
				Name = "Wrath of Sora Kell",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Wrath_of_Sora_Kell",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Universal Spell Power",
								Type = "equipment",
								Value = 40
							},
							new DDOItemSetBonusProperty
							{
								Property = "Attack",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage",
								Type = "artifact",
								Value = 2
							}
						}
					}
				}
			});
			#endregion

			#region Subterrane Raid and Dragontouched sets
			Sets.Add("Glacial Assault", new DDOItemSet
			{
				Name = "Glacial Assault",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Glacial_Assault",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Power",
								Type = "equipment",
								Value = 72
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Power",
								Type = "equipment",
								Value = 78
							}
						}
					}
				}
			});

			Sets.Add("Levik's Defender", new DDOItemSet
			{
				Name = "Levik's Defender",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Levik.27s_Defender",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class",
								Type = "insight",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Threat Generation",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fortitude",
								Type = "insight",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Will",
								Type = "insight",
								Value = 1
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class",
								Type = "insight",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Threat Generation",
								Value = 30
							}
						}
					}
				}
			});

			Sets.Add("Lorikk's Champion", new DDOItemSet
			{
				Name = "Lorik's Champion",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Lorikk.27s_Champion",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Power",
								Type = "equipment",
								Value = 72
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Power",
								Type = "equipment",
								Value = 78
							}
						}
					}
				}
			});

			Sets.Add("Tharne's Wrath", new DDOItemSet
			{
				Name = "Tharne's Wrath",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Tharne.27s_Wrath",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Ghost Touch"
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Threat Reduction",
								Value = 20
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Ghost Touch"
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Threat Reduction",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Feather Falling"
							}
						}
					}
				}
			});
			#endregion

			#region Prestige Enhancements sets
			Sets.Add("Dragonmark Heir", new DDOItemSet
			{
				Name = "Dragonmark Heir",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Dragonmark_Heir",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Greater Dragonmark Uses",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Frenzied Berserker", new DDOItemSet
			{
				Name = "Frenzied Berserker",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Frenzied_Berserker",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Rage Uses",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Strength",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Constitution",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Occult Slayer", new DDOItemSet
			{
				Name = "Occult Slayer",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Occult_Slayer",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Will",
								Type = "exceptional",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Reflex",
								Type = "exceptional",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Spell Resistance",
								Type = "enhancement",
								Value = 22
							}
						}
					}
				}
			});

			Sets.Add("Ravager", new DDOItemSet
			{
				Name = "Ravager",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Ravager",
				SetBonuses = new List<DDOItemSetBonus>()
			});

			Sets.Add("Spell Singer", new DDOItemSet
			{
				Name = "Spell Singer",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Spell_Singer",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Bard Song Uses",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Virtuoso", new DDOItemSet
			{
				Name = "Virtuoso",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Virtuoso",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Bard Song Uses",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Extend Spell Point Reduction",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Warchanter", new DDOItemSet
			{
				Name = "Warchanter",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Warchanter",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Bard Song Uses",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Maximize Spell Point Reduction",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Extend Spell Point Reduction",
								Value = 1
							}
						}
					}
				}
			});

			Sets.Add("Exorcist of the Silver Flame", new DDOItemSet
			{
				Name = "Exorcist of the Silver Flame",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Exorcist_of_the_Silver_Flame",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Turn Undead Uses",
								Type = "enhancement",
								Value = 3
							}
						}
					}
				}
			});

			Sets.Add("Radiant Servant", new DDOItemSet
			{
				Name = "Radiant Servant",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Radiant_Servant",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Turn Undead Uses",
								Type = "enhancement",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Turn Undead Effective Level",
								Type = "exceptional",
								Value = 1
							}
						}
					}
				}
			});

			Sets.Add("Warpriest", new DDOItemSet
			{
				Name = "Warpriest",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Warpriest",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Turn Undead Uses",
								Type = "enhancement",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Attack",
								Type = "exceptional",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Kensai", new DDOItemSet
			{
				Name = "Kensai",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Kensai",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Melee Attack",
								Type = "exceptional",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Confirm Critical Hits",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Attack",
								Type = "exceptional",
								Value = 3
							}
						}
					}
				}
			});

			Sets.Add("Purple Dragon Knight", new DDOItemSet
			{
				Name = "Purple Dragon Knight",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Purple_Dragon_Knight",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Melee Threat Generation",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Threat Generation",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Attack",
								Type = "exceptional",
								Value = 1
							}
						}
					}
				}
			});

			Sets.Add("Stalwart Defender", new DDOItemSet
			{
				Name = "Stalwart Defender",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Stalwart_Defender",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Melee Threat Generation",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Henshin Mystic", new DDOItemSet
			{
				Name = "Henshin Mystic",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Henshin_Mystic",
				SetBonuses = new List<DDOItemSetBonus>()
			});

			Sets.Add("Ninja Spy", new DDOItemSet
			{
				Name = "Ninja Spy",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Ninja_Spy",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Attack",
								Type = "exceptional",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Damage",
								Type = "exceptional",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Threat Reduction",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Threat Reduction",
								Value = 20
							}
						}
					}
				}
			});

			Sets.Add("Shintao Monk", new DDOItemSet
			{
				Name = "Shintao Monk",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Shintao_Monk",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Attack",
								Type = "exceptional",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage",
								Type = "exceptional",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Threat Generation",
								Value = 15
							}
						}
					}
				}
			});

			Sets.Add("Defender of Siberys", new DDOItemSet
			{
				Name = "Defender of Siberys",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Defender_of_Siberys",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Melee Threat Generation",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Hunter of the Dead", new DDOItemSet
			{
				Name = "Hunter of the Dead",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Hunter_of_the_Dead",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Remove Disease Uses",
								Value = 3
							}
						}
					}
				}
			});

			Sets.Add("Knight of the Chalice", new DDOItemSet
			{
				Name = "Knight of the Chalice",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Knight_of_the_Chalice",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Spell Resistance",
								Type = "enhancement",
								Value = 22
							}
						}
					}
				}
			});

			Sets.Add("Arcane Archer", new DDOItemSet
			{
				Name = "Arcane Archer",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Arcane_Archer",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Attack Speed",
								Type = "competnce",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Deepwood Sniper", new DDOItemSet
			{
				Name = "Deepwood Sniper",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Deepwood_Sniper",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Attack Speed",
								Type = "competence",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Attack",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Tempest", new DDOItemSet
			{
				Name = "Tempest",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Tempest",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Attack",
								Value = 3
							}
						}
					}
				}
			});

			Sets.Add("Assassin", new DDOItemSet
			{
				Name = "Assassin",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Assassin",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Attack",
								Type = "enhancement",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Damage",
								Type = "enhancement",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Threat Reduction",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Threat Reduction",
								Value = 20
							}
						}
					}
				}
			});

			Sets.Add("Mechanic", new DDOItemSet
			{
				Name = "Mechanic",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Mechanic",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Open Lock",
								Type = "competence",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Disable Device",
								Type = "competence",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Balance",
								Type = "exceptional",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Hide",
								Type = "exceptional",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Move Silently",
								Type = "exceptional",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Open Lock",
								Type = "exceptional",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Tumble",
								Type = "exceptional",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Thief Acrobat", new DDOItemSet
			{
				Name = "Thief Acrobat",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Thief_Acrobat",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Jump",
								Type = "competence",
								Value = 15
							}
						}
					}
				}
			});

			Sets.Add("Air Savant", new DDOItemSet
			{
				Name = "Air Savant",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Air_Savant",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Critical Chance",
								Type = "artifact",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Earth Savant", new DDOItemSet
			{
				Name = "Earth Savant",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Earth_Savant",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Critical Chance",
								Type = "artifact",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Fire Savant", new DDOItemSet
			{
				Name = "Fire Savant",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Fire_Savant",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Critical Chance",
								Type = "artifact",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Water Savant", new DDOItemSet
			{
				Name = "Water Savant",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Water_Savant",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Critical Chance",
								Type = "artifact",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Archmage", new DDOItemSet
			{
				Name = "Archmage",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Archmage",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Critical Chance",
								Type = "equipment",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Critical Chance",
								Type = "equipment",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Critical Chance",
								Type = "equipment",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Critical Chance",
								Type = "equipment",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Critical Chance",
								Type = "equipment",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Critical Chance",
								Type = "equipment",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Critical Chance",
								Type = "equipment",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell Critical Chance",
								Type = "equipment",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Critical Chance",
								Type = "equipment",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Spell Critical Chance",
								Type = "equipment",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sonic Spell Critical Chance",
								Type = "equipment",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Critical Multiplier",
								Type = "equipment",
								Value = 0.5f
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Critical Multiplier",
								Type = "equipment",
								Value = 0.5f
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Critical Multiplier",
								Type = "equipment",
								Value = 0.5f
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Critical Multiplier",
								Type = "equipment",
								Value = 0.5f
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Critical Multiplier",
								Type = "equipment",
								Value = 0.5f
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Critical Multiplier",
								Type = "equipment",
								Value = 0.5f
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Critical Multiplier",
								Type = "equipment",
								Value = 0.5f
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell Critical Multiplier",
								Type = "equipment",
								Value = 0.5f
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Critical Multiplier",
								Type = "equipment",
								Value = 0.5f
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Spell Critical Multiplier",
								Type = "equipment",
								Value = 0.5f
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sonic Spell Critical Multiplier",
								Type = "equipment",
								Value = 0.5f
							}
						}
					}
				}
			});

			Sets.Add("Pale Master", new DDOItemSet
			{
				Name = "Pale Master",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Pale_Master",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Power",
								Type = "equipment",
								Value = 78
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell Power",
								Type = "equipment",
								Value = 78
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Power",
								Type = "enhancement",
								Value = 90
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell Power",
								Type = "enhancement",
								Value = 90
							}
						}
					}
				}
			});

			Sets.Add("Wild Mage", new DDOItemSet
			{
				Name = "Wild Mage",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Wild_Mage",
				SetBonuses = new List<DDOItemSetBonus>()
			});
			#endregion

			#region Secrets of the Artificers sets
			Sets.Add("Tinker's Finesse", new DDOItemSet
			{
				Name = "Tinker's Finesse",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Tinker.27s_Finesse",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "True Seeing"
							},
							new DDOItemSetBonusProperty
							{
								Property = "Attack",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Damage",
								Type = "enhancement",
								Value = 8
							}
						}
					}
				}
			});

			Sets.Add("Magewright's Expertise", new DDOItemSet
			{
				Name = "Magewright's Expertise",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Magewright.27s_Expertise",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Abjuration Spell DC",
								Type = "equipment",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Conjuration Spell DC",
								Type = "equipment",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Divination Spell DC",
								Type = "equipment",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Enchantment Spell DC",
								Type = "equipment",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "equipment",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Illusion Spell DC",
								Type = "equipment",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Necromancy Spell DC",
								Type = "equipment",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Transmutation Spell DC",
								Type = "equipment",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Universal Spell Power",
								Type = "alchemical",
								Value = 12
							}
						}
					}
				}
			});

			Sets.Add("Fabricator's Ingenuity", new DDOItemSet
			{
				Name = "Fabricator's Ingenuity",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Fabricator.27s_Ingenuity",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Fortification",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Attack",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage",
								Type = "artifact",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Alchemist's Lore", new DDOItemSet
			{
				Name = "Alchemist's Lore",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Alchemist.27s_Lore",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Empower Spell Point Reduction",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Empower Healing Spell Point Reduction",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Maximize Spell Point Reduction",
								Value = 2
							}
						}
					}
				}
			});
			#endregion

			#region Commendation sets
			Sets.Add("Amaunator's Blessing", new DDOItemSet
			{
				Name = "Amaunator's Blessing",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Amaunator.27s_Blessing",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Spell Point Cost Reduction %",
								Type = "enhancement",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Woodsman's Guile", new DDOItemSet
			{
				Name = "Woodsman's Guile",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Woodsman.27s_Guile",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Attack",
								Type = "enhancement",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Damage",
								Type = "enhancement",
								Value = 6
							}
						}
					}
				}
			});

			Sets.Add("Knight's Loyalty", new DDOItemSet
			{
				Name = "Knight's Loyalty",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Knight.27s_Loyalty",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Attack",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class",
								Type = "insight natural armor",
								Value = 3
							}
						}
					}
				}
			});

			Sets.Add("Way of the Sun Soul", new DDOItemSet
			{
				Name = "Way of the Sun Soul",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Way_of_the_Sun_Soul",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Attack",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage",
								Type = "artifact",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Battle Arcanist", new DDOItemSet
			{
				Name = "Battle Arcanist",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Battle_Arcanist",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Spell Point Cost Reduction %",
								Type = "enhancement",
								Value = 10
							}
						}
					}
				}
			});
			#endregion

			#region Planar Focus sets
			Sets.Add("Planar Focus: Erudition", new DDOItemSet
			{
				Name = "Planar Focus: Erudition",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Planar_Focus_Item_Sets",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Spell Penetration",
								Type = "equipment",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Spell Points",
								Type = "enhancement",
								Value = 250
							},
							new DDOItemSetBonusProperty
							{
								Property = "Universal Spell Power",
								Type = "psionic",
								Value = 15
							}
						}
					}
				}
			});

			Sets.Add("Planar Focus: Prowess", new DDOItemSet
			{
				Name = "Planar Focus: Prowess",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Planar_Focus_Item_Sets",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Attack",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 15
							}
						}
					}
				}
			});

			Sets.Add("Planar Focus: Subterfuge", new DDOItemSet
			{
				Name = "Planar Focus: Subterfuge",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Planar_Focus:_Subterfuge",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Attack",
								Type = "insight",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Damage",
								Type = "insight",
								Value = 8
							},
							new DDOItemSetBonusProperty
							{
								Property = "True Seeing"
							},
							new DDOItemSetBonusProperty
							{
								Property = "Dodge",
								Type = "enhancement",
								Value = 3
							}
						}
					}
				}
			});
			#endregion

			#region Dragonscale Armor sets
			Sets.Add("Draconic Ferocity", new DDOItemSet
			{
				Name = "Draconic Ferocity",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Draconic_Ferocity",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Doublestrike",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Attack",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Damage",
								Type = "artifact",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Draconic Mind", new DDOItemSet
			{
				Name = "Draconic Mind",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Draconic_Mind",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Universal Spell Power",
								Type = "artifact",
								Value = 15
							}
						}
					}
				}
			});

			Sets.Add("Draconic Resilience", new DDOItemSet
			{
				Name = "Draconic Resilience",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Draconic_Resilience",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Hit Points",
								Type = "artifact",
								Value = 50
							}
						}
					}
				}
			});
			#endregion

			#region Iconic Level 15 Reward sets
			Sets.Add("Risk and Reward", new DDOItemSet
			{
				Name = "Risk and Reward",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Risk_and_Reward",
				SetBonuses = new List<DDOItemSetBonus>()
			});
			#endregion

			#region Unbreakable Adamancy set
			Sets.Add("Unbreakable Adamancy", new DDOItemSet
			{
				Name = "Unbreakable Adamancy",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Unbreakable_Adamancy",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "luck",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "luck",
								Value = 5
							}
						}
					}
				}
			});
			#endregion

			#region The Devil's Gambits sets
			Sets.Add("Captain's Set", new DDOItemSet
			{
				Name = "Captain's Set",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Captain.27s_Set",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Strength",
								Type = "quality",
								Value = 1
							}
						}
					}
				}
			});

			Sets.Add("Epic Captain's Set", new DDOItemSet
			{
				Name = "Epic Captain's Set",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Epic_Captain.27s_Set",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Strength",
								Type = "quality",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Double Helix Set", new DDOItemSet
			{
				Name = "Double Helix Set",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Double_Helix_Set",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "insight",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "insight",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Epic Double Helix Set", new DDOItemSet
			{
				Name = "Epic Double Helix Set",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Epic_Double_Helix_Set",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "insight",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "insight",
								Value = 5
							}
						}
					}
				}
			});

			Sets.Add("Griffon Set", new DDOItemSet
			{
				Name = "Griffon Set",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Griffon_Set",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Constitution",
								Type = "quality",
								Value = 1
							}
						}
					}
				}
			});

			Sets.Add("Epic Griffon Set", new DDOItemSet
			{
				Name = "Epic Griffon Set",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Epic_Griffon_Set",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Constitution",
								Type = "quality",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Slice 'n Dice Set", new DDOItemSet
			{
				Name = "Slice 'n Dice Set",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Slice_.27n_Dice_Set",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Confirm Critical Hits",
								Type = "quality",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Critical Hit Damage",
								Type = "quality",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Attack",
								Type = "quality",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Damage",
								Type = "quality",
								Value = 1
							}
						}
					}
				}
			});

			Sets.Add("Epic Slice 'n Dice Set", new DDOItemSet
			{
				Name = "Epic Slice 'n Dice Set",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Epic_Slice_.27n_Dice_Set",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Confirm Critical Hits",
								Type = "quality",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Critical Hit Damage",
								Type = "quality",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Attack",
								Type = "quality",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Damage",
								Type = "quality",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("The Devil's Handiwork", new DDOItemSet
			{
				Name = "The Devil's Handiwork",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#The_Devil.27s_Handiwork",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 5,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Strength",
								Type = "quality",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Dexterity",
								Type = "quality",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Constitution",
								Type = "quality",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Intelligence",
								Type = "quality",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Wisdom",
								Type = "quality",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Charisma",
								Type = "quality",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Epic The Devil's Handiwork", new DDOItemSet
			{
				Name = "Epic The Devil's Handiwork",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Epic_The_Devil.27s_Handiwork",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 5,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Strength",
								Type = "quality",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Dexterity",
								Type = "quality",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Constitution",
								Type = "quality",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Intelligence",
								Type = "quality",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Wisdom",
								Type = "quality",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Charisma",
								Type = "quality",
								Value = 3
							}
						}
					}
				}
			});
			#endregion

			#region Slave Lords Crafting sets
			Sets.Add("Slave Lord's Might", new DDOItemSet
			{
				Name = "Slave Lord's Might",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Slave_Lord.27s_Might",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact competence",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact competence",
								Value = 1
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 5,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Strength",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Dexterity",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact competence",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact competence",
								Value = 2
							}
						}
					},
				}
			});

			Sets.Add("Legendary Slave Lord's Might", new DDOItemSet
			{
				Name = "Legendary Slave Lord's Might",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Slave_Lord.27s_Might",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact competence",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact competence",
								Value = 2
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 5,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Strength",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Dexterity",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact competence",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact competence",
								Value = 4
							}
						}
					},
				}
			});

			Sets.Add("Slave Lord's Sorcery", new DDOItemSet
			{
				Name = "Slave Lord's Sorcery",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Slave_Lord.27s_Sorcery",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Power",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Power",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Power",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Power",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Power",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Power",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Power",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell Power",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Power",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Spell Power",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sonic Spell Power",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Abjuration Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Conjuration Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Divination Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Enchantment Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Illusion Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Necromancy Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Transmutation Spell DC",
								Type = "artifact",
								Value = 1
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 5,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Intelligence",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Wisdom",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Charisma",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Spell Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sonic Spell Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Abjuration Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Conjuration Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Divination Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Enchantment Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Illusion Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Necromancy Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Transmutation Spell DC",
								Type = "artifact",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Legendary Slave Lord's Sorcery", new DDOItemSet
			{
				Name = "Legendary Slave Lord's Sorcery",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Slave_Lord.27s_Sorcery",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Spell Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sonic Spell Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Abjuration Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Conjuration Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Divination Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Enchantment Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Illusion Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Necromancy Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Transmutation Spell DC",
								Type = "artifact",
								Value = 2
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 5,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Intelligence",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Wisdom",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Charisma",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Spell Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sonic Spell Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Abjuration Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Conjuration Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Divination Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Enchantment Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Illusion Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Necromancy Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Transmutation Spell DC",
								Type = "artifact",
								Value = 4
							}
						}
					}
				}
			});

			Sets.Add("Slave's Endurance", new DDOItemSet
			{
				Name = "Slave's Endurance",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Slave.27s_Endurance",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fortitude",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Reflex",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Will",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Spell Save",
								Type = "artifact",
								Value = 1
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 5,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Constitution",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fortitude",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Reflex",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Will",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Spell Saves",
								Type = "artifact",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Legendary Slave's Endurance", new DDOItemSet
			{
				Name = "Legendary Slave's Endurance",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Slave.27s_Endurance",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fortitude",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Reflex",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Will",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Spell Saves",
								Type = "artifact",
								Value = 2
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 5,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Constitution",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fortitude",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Reflex",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Will",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Spell Saves",
								Type = "artifact",
								Value = 4
							}
						}
					}
				}
			});
			#endregion

			#region Ravenloft sets
			Sets.Add("Beacon of Magic Set (Heroic)", new DDOItemSet
			{
				Name = "Beacon of Magic Set (Heroic)",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Beacon_of_Magic_Set_.28Heroic.29",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Universal Spell Power",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sonic Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating Cap",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Missile Deflection",
								Type = "artifact",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Beacon of Magic Set (Legendary)", new DDOItemSet
			{
				Name = "Beacon of Magic Set (Legendary)",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Beacon_of_Magic_Set_.28Legendary.29",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Universal Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sonic Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating Cap",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Missile Deflection",
								Type = "artifact",
								Value = 5
							}
						}
					}
				}
			});

			Sets.Add("Knight of the Shadows Set (Heroic)", new DDOItemSet
			{
				Name = "Knight of the Shadows Set (Heroic)",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Knight_of_the_Shadows_Set_.28Heroic.29",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Threat Generation",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Threat Generation",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class %",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Knight of the Shadows Set (Legendary)", new DDOItemSet
			{
				Name = "Knight of the Shadows Set (Legendary)",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Knight_of_the_Shadows_Set_.28Legendary.29",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Threat Generation",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Threat Generation",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class %",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Crypt Raider Set (Heroic)", new DDOItemSet
			{
				Name = "Crypt Raider Set (Heroic)",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Crypt_Raider_Set_.28Heroic.29",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Attack vs Evil",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage vs Evil",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Threat Reduction",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Threat Reduction",
								Type = "artifact",
								Value = 20
							}
						}
					}
				}
			});

			Sets.Add("Crypt Raider Set (Legendary)", new DDOItemSet
			{
				Name = "Crypt Raider Set (Legendary)",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Crypt_Raider_Set_.28Legendary.29",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Attack vs Evil",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage vs Evil",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Threat Reduction",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Threat Reduction",
								Type = "artifact",
								Value = 20
							}
						}
					}
				}
			});

			Sets.Add("Silent Avenger Set (Heroic)", new DDOItemSet
			{
				Name = "Silent Avenger Set (Heroic)",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Silent_Avenger_Set_.28Heroic.29",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Doublestrike",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doubleshot",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Dice",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fortification Bypass",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage vs Helpless %",
								Type = "artifact",
								Value = 5
							}
						}
					}
				}
			});

			Sets.Add("Silent Avenger Set (Legendary)", new DDOItemSet
			{
				Name = "Silent Avenger Set (Legendary)",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Silent_Avenger_Set_.28Legendary.29",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Doublestrike",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doubleshot",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Dice",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fortification Bypass",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage vs Helpless %",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Adherent of the Mists Set (Heroic)", new DDOItemSet
			{
				Name = "Adherent of the Mists Set (Heroic)",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Adherent_of_the_Mists_Set_.28Heroic.29",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 5,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "profane",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Healing Amplification",
								Type = "profane",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Healing Amplification",
								Type = "profane",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Healing Amplification",
								Type = "profane",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "profane",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "profane",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Universal Spell Power",
								Type = "profane",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Adherent of the Mists Set (Legendary)", new DDOItemSet
			{
				Name = "Adherent of the Mists Set (Legendary)",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Adherent_of_the_Mists_Set_.28Legendary.29",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 5,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "profane",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Healing Amplification",
								Type = "profane",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Healing Amplification",
								Type = "profane",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Healing Amplification",
								Type = "profane",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "profane",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "profane",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Universal Spell Power",
								Type = "profane",
								Value = 20
							}
						}
					}
				}
			});

			Sets.Add("Pain and Suffering", new DDOItemSet
			{
				Name = "Pain and Suffering",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Pain_and_Suffering",
				SetBonuses = new List<DDOItemSetBonus>()
			});
			#endregion

			#region Disciples of Rage sets
			Sets.Add("Wayward Warrior", new DDOItemSet
			{
				Name = "Wayward Warrior",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Wayward_Warrior",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class",
								Type = "artifact natural armor",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Constitution",
								Type = "artifact",
								Value = 1
							}
						}
					}
				}
			});

			Sets.Add("Wayward Warrior (Legendary)", new DDOItemSet
			{
				Name = "Wayward Warrior (Legendary)",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Wayward_Warrior_.28Legendary.29",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class",
								Type = "artifact natural armor",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Constitution",
								Type = "artifact",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Seasons of Change", new DDOItemSet
			{
				Name = "Seasons of Change",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Seasons_of_Change",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "artifact",
								Value = 1
							}
						}
					}
				}
			});

			Sets.Add("Seasons of Change (Legendary)", new DDOItemSet
			{
				Name = "Seasons of Change (Legendary)",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Seasons_of_Change_.28Legendary.29",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Power",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Power",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "artifact",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Renegade Champion", new DDOItemSet
			{
				Name = "Renegade Champion",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Renegade_Champion",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Critical Multiplier on 19-20",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Rune Arm DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Healing Amplification",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Renegade Champion (Legendary)", new DDOItemSet
			{
				Name = "Renegade Champion (Legendary)",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Renegade_Champion_.28Legendary.29",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Critical Multiplier on 19-20",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Rune Arm DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Healing Amplification",
								Type = "artifact",
								Value = 20
							}
						}
					}
				}
			});

			Sets.Add("Heavy Warfare", new DDOItemSet
			{
				Name = "Heavy Warfare",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Heavy_Warfare",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doublestrike",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Offhand Doublestrike",
								Type = "artifact",
								Value = 5
							}
						}
					}
				}
			});

			Sets.Add("Heavy Warfare (Legendary)", new DDOItemSet
			{
				Name = "Heavy Warfare (Legendary)",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Heavy_Warfare_.28Legendary.29",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doublestrike",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Offhand Doublestrike",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Curse Necromancer", new DDOItemSet
			{
				Name = "Curse Necromancer",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Curse_Necromancer",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Negative Healing Amplification",
								Type = "artifact",
								Value = 5
							}
						}
					}
				}
			});

			Sets.Add("Curse Necromancer (Legendary)", new DDOItemSet
			{
				Name = "Curse Necromancer (Legendary)",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Curse_Necromancer_.28Legendary.29",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Negative Healing Amplification",
								Type = "artifact",
								Value = 15
							}
						}
					}
				}
			});
			#endregion

			#region Killing Time sets
			Sets.Add("Brilliant Crescents", new DDOItemSet
			{
				Name = "Brilliant Crescents",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Brilliant_Crescents",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Offhand Strike Chance",
								Value = 20
							}
						}
					}
				}
			});

			Sets.Add("Mountainskin Set", new DDOItemSet
			{
				Name = "Mountainskin Set",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Mountainskin_Set",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Power",
								Type = "exceptional",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Critical Chance",
								Type = "exceptional",
								Value = 15
							}
						}
					}
				}
			});
			#endregion

			#region Masterminds of Sharn sets
			Sets.Add("Arcsteel Battlemage", new DDOItemSet
			{
				Name = "Arcsteel Battlemage",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Arcsteel_Battlemage",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Power",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Power",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Power",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Spell Power",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Critical Chance",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Critical Chance",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Critical Chance",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Spell Critical Chance",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Intelligence",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Abjuration Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Conjuration Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Divination Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Enchantment Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Illusion Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Necromancy Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Transmutation Spell DC",
								Type = "artifact",
								Value = 1
							}
						}
					}
				}
			});

			Sets.Add("Legendary Arcsteel Battlemage", new DDOItemSet
			{
				Name = "Legendary Arcsteel Battlemage",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Arcsteel_Battlemage",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Intelligence",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Abjuration Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Conjuration Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Divination Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Enchantment Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Illusion Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Necromancy Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Transmutation Spell DC",
								Type = "artifact",
								Value = 3
							}
						}
					}
				}
			});

			Sets.Add("Esoteric Initiate", new DDOItemSet
			{
				Name = "Esoteric Initiate",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Esoteric_Initiate",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Universal Spell Power",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Intelligence",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Wisdom",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Charisma",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Abjuration Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Conjuration Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Divination Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Enchantment Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Illusion Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Necromancy Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Transmutation Spell DC",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating Cap",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Legendary Esoteric Initiate", new DDOItemSet
			{
				Name = "Legendary Esoteric Initiate",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Esoteric_Initiate",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Universal Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Intelligence",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Wisdom",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Charisma",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Abjuration Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Conjuration Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Divination Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Enchantment Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Illusion Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Necromancy Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Transmutation Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating Cap",
								Value = 20
							}
						}
					}
				}
			});

			Sets.Add("Flamecleansed Fury", new DDOItemSet
			{
				Name = "Flamecleansed Fury",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Flamecleansed_Fury",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Power",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Power",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Power",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Power",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Critical Chance",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Critical Chance",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Critical Chance",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Critical Chance",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Wisdom",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Charisma",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Abjuration Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Conjuration Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Divination Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Enchantment Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Illusion Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Necromancy Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Transmutation Spell DC",
								Type = "artifact",
								Value = 1
							}
						}
					}
				}
			});

			Sets.Add("Legendary Flamecleansed Fury", new DDOItemSet
			{
				Name = "Legendary Flamecleansed Fury",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Flamecleansed_Fury",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Wisdom",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Charisma",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Abjuration Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Conjuration Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Divination Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Enchantment Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Illusion Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Necromancy Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Transmutation Spell DC",
								Type = "artifact",
								Value = 3
							}
						}
					}
				}
			});

			Sets.Add("Guardian of the Gates", new DDOItemSet
			{
				Name = "Guardian of the Gates",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Guardian_of_the_Gates",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class %",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Threat Generation",
								Type = "artifact",
								Value = 75
							},
							new DDOItemSetBonusProperty
							{
								Property = "Acid Absorption",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Absorption",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Absorption",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Absorption",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Legendary Guardian of the Gates", new DDOItemSet
			{
				Name = "Legendary Guardian of the Gates",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Guardian_of_the_Gates",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class %",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Threat Generation",
								Type = "artifact",
								Value = 75
							},
							new DDOItemSetBonusProperty
							{
								Property = "Acid Absorption",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Absorption",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Absorption",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Absorption",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Hruit's Influence", new DDOItemSet
			{
				Name = "Hruit's Influence",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Hruit.27s_Influence",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Power",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Power",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Power",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Power",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Critical Chance",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Critical Chance",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Critical Chance",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Critical Chance",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Wisdom",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Abjuration Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Conjuration Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Divination Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Enchantment Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Illusion Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Necromancy Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Transmutation Spell DC",
								Type = "artifact",
								Value = 1
							}
						}
					}
				}
			});

			Sets.Add("Legendary Hruit's Influence", new DDOItemSet
			{
				Name = "Legendary Hruit's Influence",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Hruit.27s_Influence",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Wisdom",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Abjuration Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Conjuration Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Divination Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Enchantment Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Illusion Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Necromancy Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Transmutation Spell DC",
								Type = "artifact",
								Value = 3
							}
						}
					}
				}
			});

			Sets.Add("Part of the Family", new DDOItemSet
			{
				Name = "Part of the Family",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Part_of_the_Family",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Doublestrike",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage vs Helpless %",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fortification Bypass",
								Type = "artifact",
								Value = 5
							}
						}
					}
				}
			});

			Sets.Add("Legendary Part of the Family", new DDOItemSet
			{
				Name = "Legendary Part of the Family",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Part_of_the_Family",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Doublestrike",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage vs Helpless %",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fortification Bypass",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Wallwatch", new DDOItemSet
			{
				Name = "Wallwatch",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Wallwatch",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Dice",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fortification Bypass",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doubleshot",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Legendary Wallwatch", new DDOItemSet
			{
				Name = "Legendary Wallwatch",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Wallwatch",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Dice",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fortification Bypass",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doubleshot",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 20
							}
						}
					}
				}
			});

			Sets.Add("Inevitable Balance", new DDOItemSet
			{
				Name = "Inevitable Balance",
				WikiURL = "",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doublestrike",
								Value = 5
							}
						}
					}
				}
			});

			Sets.Add("Legendary Inevitable Balance", new DDOItemSet
			{
				Name = "Legendary Inevitable Balance",
				WikiURL = "",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doublestrike",
								Value = 10
							}
						}
					}
				}
			});
			#endregion

			#region Soul Splitter sets
			Sets.Add("Dreadkeeper", new DDOItemSet
			{
				Name = "Dreadkeeper",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Dreadkeeper",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Power",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Power",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Critical Chance",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Critical Chance",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Intelligence",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Abjuration Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Conjuration Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Divination Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Enchantment Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Illusion Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Necromancy Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Transmutation Spell DC",
								Type = "artifact",
								Value = 1
							}
						}
					}
				}
			});

			Sets.Add("Legendary Dreadkeeper", new DDOItemSet
			{
				Name = "Legendary Dreadkeeper",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Dreadkeeper",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Intelligence",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Abjuration Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Conjuration Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Divination Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Enchantment Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Illusion Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Necromancy Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Transmutation Spell DC",
								Type = "artifact",
								Value = 3
							}
						}
					}
				}
			});

			Sets.Add("Feywild Dreamer", new DDOItemSet
			{
				Name = "Feywild Dreamer",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Feywild_Dreamer",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Power",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Power",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Power",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sonic Spell Power",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Critical Chance",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Critical Chance",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Critical Chance",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sonic Spell Critical Chance",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Charisma",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Abjuration Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Conjuration Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Divination Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Enchantment Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Illusion Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Necromancy Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Transmutation Spell DC",
								Type = "artifact",
								Value = 1
							}
						}
					}
				}
			});

			Sets.Add("Legendary Feywild Dreamer", new DDOItemSet
			{
				Name = "Legendary Feywild Dreamer",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Feywild_Dreamer",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sonic Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sonic Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Charisma",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Abjuration Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Conjuration Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Divination Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Enchantment Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Illusion Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Necromancy Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Transmutation Spell DC",
								Type = "artifact",
								Value = 3
							}
						}
					}
				}
			});

			Sets.Add("Profane Experiment", new DDOItemSet
			{
				Name = "Profane Experiment",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Profane_Experiment",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Dice",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doublestrike",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doubleshot",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Constitution",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Intelligence",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Universal Spell Power",
								Type = "artifact",
								Value = 25
							}
						}
					}
				}
			});

			Sets.Add("Legendary Profane Experiment", new DDOItemSet
			{
				Name = "Legendary Profane Experiment",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Profane_Experiment",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Dice",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doublestrike",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doubleshot",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Constitution",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Intelligence",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Universal Spell Power",
								Type = "artifact",
								Value = 50
							}
						}
					}
				}
			});
			#endregion

			// these aren't really sets, but the easiest way to model them is by defining them as sets
			// that are options for the items that can have them
			#region Thunder-Forged abilities
			Sets.Add("Shadow Caster", new DDOItemSet
			{
				Name = "Shadow Caster",
				WikiURL = "",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 1,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Concentration",
								Type = "profane",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Abjuration Spell DC",
								Type = "profane",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Conjuration Spell DC",
								Type = "profane",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Divination Spell DC",
								Type = "profane",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Enchantment Spell DC",
								Type = "profane",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Evocation Spell DC",
								Type = "profane",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Illusion Spell DC",
								Type = "profane",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Necromancy Spell DC",
								Type = "profane",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Transmutation Spell DC",
								Type = "profane",
								Value = 1
							}
						}
					}
				}
			});

			Sets.Add("Shadow Disciple", new DDOItemSet
			{
				Name = "Shadow Disciple",
				WikiURL = "",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 1,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Concentration",
								Type = "profane",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Trip DC",
								Type = "profane",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sunder DC",
								Type = "profane",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Stunning DC",
								Type = "profane",
								Value = 1
							}
						}
					}
				}
			});

			Sets.Add("Shadow Killer", new DDOItemSet
			{
				Name = "Shadow Killer",
				WikiURL = "",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 1,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Damage",
								Type = "profane",
								Value = 12 // this is really 2d6
							},
							new DDOItemSetBonusProperty
							{
								Property = "Reflex",
								Type = "profane",
								Value = 1
							}
						}
					}
				}
			});

			Sets.Add("Shadow Striker", new DDOItemSet
			{
				Name = "Shadow Striker",
				WikiURL = "",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 1,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Doublestrike",
								Type = "profane",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Attack Speed",
								Type = "enhancement",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Attack Speed",
								Type = "enhancement",
								Value = 20
							}
						}
					}
				}
			});

			Sets.Add("Shadow Guardian", new DDOItemSet
			{
				Name = "Shadow Guardian",
				WikiURL = "",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 1,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Damage Reduction",
								Type = "epic",
								Value = 60
							}
						}
					}
				}
			});

			Sets.Add("Shadow Construct", new DDOItemSet
			{
				Name = "Shadow Construct",
				WikiURL = "",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 1,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Repair Healing Amplification",
								Type = "profane",
								Value = 10
							}
						}
					}
				}
			});
			#endregion

			// there are only two items in the game that use this
			#region Mysterious Effects
			Sets.Add("Mysterious Effect Option 1", new DDOItemSet
			{
				Name = "Mysterious Effect Option 1",
				WikiURL = null,
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Dexterity",
								Type = "enhancement",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fortitude",
								Type = "resistance",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Reflex",
								Type = "resistance",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Will",
								Type = "resistance",
								Value = 3
							}
						}
					}
				}
			});

			Sets.Add("Mysterious Effect Option 2", new DDOItemSet
			{
				Name = "Mysterious Effect Option 2",
				WikiURL = null,
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Charisma",
								Type = "enhancement",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fortification",
								Type = "enhancement",
								Value = 75
							}
						}
					}
				}
			});

			Sets.Add("Mysterious Effect Option 3", new DDOItemSet
			{
				Name = "Mysterious Effect Option 3",
				WikiURL = null,
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Strength",
								Type = "enhancement",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class",
								Type = "deflection",
								Value = 3
							}
						}
					}
				}
			});

			Sets.Add("Mysterious Effect Option 4", new DDOItemSet
			{
				Name = "Mysterious Effect Option 4",
				WikiURL = null,
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Constitution",
								Type = "enhancement",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Spell Penetration",
								Type = "equipment",
								Value = 1
							}
						}
					}
				}
			});
            #endregion

            Sets.Add("Legendary Green Steel", new LGSItemSet());

			#region Legendary Vision of Destruction sets
			Sets.Add("Legacy of Lorikk", new DDOItemSet
			{
				Name = "Legacy of Lorikk",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legacy_of_Lorikk",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Crit Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Crit Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell Crit Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell Crit Chance",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Legacy of Levikk", new DDOItemSet
			{
				Name = "Legacy of Levikk",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legacy_of_Levikk",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class %",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Threat Generation",
								Type = "artifact",
								Value = 75
							}
						}
					}
				}
			});

			Sets.Add("Mind and Matter", new DDOItemSet
			{
				Name = "Mind and Matter",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Mind_and_Matter",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating Cap",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Healing Amplification",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Healing Amplification",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Healing Amplification",
								Type = "artifact",
								Value = 10
							},
						}
					}
				}
			});

			Sets.Add("Legacy of Tharne", new DDOItemSet
			{
				Name = "Legacy of Tharne",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legacy_of_Tharne",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Doublestrike",
								Type = "artifact",
								Value = 10,
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doubleshot",
								Type = "artifact",
								Value = 10,
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage vs Helpless %",
								Type = "artifact",
								Value = 10,
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Dice",
								Type = "artifact",
								Value = 3,
							},
							new DDOItemSetBonusProperty
							{
								Property = "Search",
								Type = "artifact",
								Value = 5,
							},
							new DDOItemSetBonusProperty
							{
								Property = "Spot",
								Type = "artifact",
								Value = 5,
							},
						}
					}
				}
			});

			Sets.Add("Anger of the Avalanche", new DDOItemSet
			{
				Name = "Anger of the Avalanche",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Anger_of_the_Avalanche",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sonic Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sonic Spell Critical Chance",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Mantle of Suulomades", new DDOItemSet
			{
				Name = "Mantle of Suulomades",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Mantle_of_Suulomades",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Dice",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doublestrike",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doubleshot",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("One with the Swarm", new DDOItemSet
			{
				Name = "One with the Swarm",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#One_with_the_Swarm",
				SetBonuses = new List<DDOItemSetBonus>
				{
				}
			});
			#endregion

			#region Legendary Lord of Blades and Legendary Master Artificer sets
			Sets.Add("Chained Elementals", new DDOItemSet
			{
				Name = "Chained Elementals",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Chained_Elementals",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Critical Chance",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Tyrannical Tinkerer", new DDOItemSet
			{
				Name = "Tyrannical Tinkerer",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Tyrannical_Tinkerer",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Dice",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fortification Bypass",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Open Lock",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Disable Device",
								Type = "artifact",
								Value = 5
							}
						}
					}
				}
			});

			Sets.Add("Masterful Magewright", new DDOItemSet
			{
				Name = "Masterful Magewright",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Masterful_Magewright",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sonic Spell DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Intelligence",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Wisdom",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Charisma",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Perform",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Concentration",
								Type = "artifact",
								Value = 5
							}
						}
					}
				}
			});

			Sets.Add("Fastidious Fabricator", new DDOItemSet
			{
				Name = "Fastidious Fabricator",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Fastidious_Fabricator",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class %",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating Cap",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Balance",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair",
								Type = "artifact",
								Value = 5
							}
						}
					}
				}
			});

			Sets.Add("Astute Alchemist", new DDOItemSet
			{
				Name = "Astute Alchemist",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Astute_Alchemist",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell Critical Chance",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Conduit of the Titans", new DDOItemSet
			{
				Name = "Conduit of the Titans",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Conduit_of_the_Titans",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Rune Arm DC",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Spell Power",
								Type = "artifact",
								Value = 50
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Critical Chance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Spell Critical Chance",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});
			#endregion

			#region Feywild sets
			Sets.Add("Seasons of the Feywild", new DDOItemSet
			{
				Name = "Seasons of the Feywild",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Seasons_of_the_Feywild",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Hit Points",
								Type = "artifact",
								Value = 10
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Spell Points",
								Type = "artifact",
								Value = 50
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 4,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Strength",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Dexterity",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Constitution",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Intelligence",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Wisdom",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Charisma",
								Type = "artifact",
								Value = 1
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 5,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Dodge",
								Type = "artifact",
								Value = 1
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 6,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Spell DC",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sonic Spell DC",
								Type = "artifact",
								Value = 1
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 7,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 5
							}
						}
					},
				}
			});

			Sets.Add("Eminence of Autumn", new DDOItemSet
			{
				Name = "Eminence of Autumn",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Eminence_of_Autumn",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Universal Spell Power",
								Type = "artifact",
								Value = 50
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Universal Spell Critical Chance",
								Type = "artifact",
								Value = 50
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 4,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Maximum Spell Points %",
								Type = "legendary",
								Value = 20
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 5,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Intelligence",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Wisdom",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Charisma",
								Type = "artifact",
								Value = 4
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 6,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Acid Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Cold Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Light Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Spell DC",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sonic Spell DC",
								Type = "artifact",
								Value = 3
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 7,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Universal Spell Critical Damage",
								Type = "legendary",
								Value = 15
							}
						}
					},
				}
			});

			Sets.Add("Eminence of Spring", new DDOItemSet
			{
				Name = "Eminence of Spring",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Eminence_of_Spring",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Missile Deflection",
								Type = "artifact",
								Value = 10
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Damage vs Helpless %",
								Type = "artifact",
								Value = 15
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 4,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Dodge Cap",
								Type = "legendary",
								Value = 3
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 5,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Dice",
								Type = "",
								Value = 3
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 6,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Doublestrike",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doubleshot",
								Type = "artifact",
								Value = 15
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 7,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 25
							}
						}
					}
				}
			});

			Sets.Add("Eminence of Summer", new DDOItemSet
			{
				Name = "Eminence of Summer",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Eminence_of_Summer",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 25
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Fortification Bypass",
								Type = "artifact",
								Value = 25
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 4,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Trip DC",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Stun DC",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sunder DC",
								Type = "artifact",
								Value = 5
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 5,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Damage vs Helpless %",
								Type = "artifact",
								Value = 15
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 6,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Doublestrike",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doubleshot",
								Type = "artifact",
								Value = 15
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 7,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 25
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 25
							}
						}
					},
				}
			});

			Sets.Add("Eminence of Winter", new DDOItemSet
			{
				Name = "Eminence of Winter",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Eminence_of_Winter",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 25
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 3,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Positive Healing Amplification",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Healing Amplification",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Healing Amplification",
								Type = "artifact",
								Value = 15
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 4,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Maximum Hit Points %",
								Type = "legendary",
								Value = 20
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 5,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Melee Threat Generation",
								Type = "artifact",
								Value = 100
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Threat Generation",
								Type = "artifact",
								Value = 100
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 6,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Constitution",
								Type = "artifact",
								Value = 4
							}
						}
					},
					new DDOItemSetBonus
					{
						MinimumItems = 7,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class %",
								Type = "artifact",
								Value = 15
							}
						}
					}
				}
			});
			#endregion

			#region Demon Sands sets
			Sets.Add("Oasis of Morality", new DDOItemSet
			{
				Name = "Oasis of Morality",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Oasis_of_Morality",
				SetBonuses = new List<DDOItemSetBonus>
					{
						new DDOItemSetBonus
						{
							MinimumItems = 2,
							Bonuses = new List<DDOItemSetBonusProperty>
							{
								new DDOItemSetBonusProperty
								{
									Property = "Positive Spell Critical Chance",
									Type = "artifact",
									Value = 2
								},
								new DDOItemSetBonusProperty
								{
									Property = "Positive Spell Power",
									Type = "artifact",
									Value = 10
								},
								new DDOItemSetBonusProperty
								{
									Property = "Fortification Bypass",
									Type = "artifact",
									Value = 5
								}
							}
						}
					}
			});

			Sets.Add("Epic Oasis of Morality", new DDOItemSet
			{
				Name = "Epic Oasis of Morality",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Epic_Oasis_of_Morality",
				SetBonuses = new List<DDOItemSetBonus>
					{
						new DDOItemSetBonus
						{
							MinimumItems = 2,
							Bonuses = new List<DDOItemSetBonusProperty>
							{
								new DDOItemSetBonusProperty
								{
									Property = "Positive Spell Critical Chance",
									Type = "artifact",
									Value = 4
								},
								new DDOItemSetBonusProperty
								{
									Property = "Positive Spell Power",
									Type = "artifact",
									Value = 20
								},
								new DDOItemSetBonusProperty
								{
									Property = "Fortification Bypass",
									Type = "artifact",
									Value = 10
								}
							}
						}
					}
			});

			Sets.Add("Legendary Oasis of Morality", new DDOItemSet
			{
				Name = "Legendary Oasis of Morality",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Oasis_of_Morality",
				SetBonuses = new List<DDOItemSetBonus>
					{
						new DDOItemSetBonus
						{
							MinimumItems = 2,
							Bonuses = new List<DDOItemSetBonusProperty>
							{
								new DDOItemSetBonusProperty
								{
									Property = "Positive Spell Critical Chance",
									Type = "artifact",
									Value = 6
								},
								new DDOItemSetBonusProperty
								{
									Property = "Positive Spell Power",
									Type = "artifact",
									Value = 30
								},
								new DDOItemSetBonusProperty
								{
									Property = "Fortification Bypass",
									Type = "artifact",
									Value = 15
								}
							}
						}
					}
			});

			Sets.Add("Vulkoor's Chosen", new DDOItemSet
			{
				Name = "Vulkoor's Chosen",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Vulkoor.27s_Chosen",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Poison Resistance",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Dice",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fortitude",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Reflex",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Will",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Dexterity",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Constitution",
								Type = "artifact",
								Value = 1
							}
						}
					}
				}
			});

			Sets.Add("Epic Vulkoor's Chosen", new DDOItemSet
			{
				Name = "Epic Vulkoor's Chosen",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Epic_Vulkoor.27s_Chosen",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Poison Resistance",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Dice",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fortitude",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Reflex",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Will",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Dexterity",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Constitution",
								Type = "artifact",
								Value = 2
							}
						}
					}
				}
			});

			Sets.Add("Legendary Vulkoor's Chosen", new DDOItemSet
			{
				Name = "Legendary Vulkoor's Chosen",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Vulkoor.27s_Chosen",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Poison Resistance",
								Type = "artifact",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Dice",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fortitude",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Reflex",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Will",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Dexterity",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Constitution",
								Type = "artifact",
								Value = 3
							}
						}
					}
				}
			});

			Sets.Add("The Desert's Writhing Storm", new DDOItemSet
			{
				Name = "The Desert's Writhing Storm",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#The_Desert.27s_Writhing_Storm",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Critical Chance",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("The Epic Desert's Writhing Storm", new DDOItemSet
			{
				Name = "The Epic Desert's Writhing Storm",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#The_Epic_Desert.27s_Writhing_Storm",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Critical Chance",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Power",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 20
							}
						}
					}
				}
			});

			Sets.Add("The Legendary Desert's Writhing Storm", new DDOItemSet
			{
				Name = "The Legendary Desert's Writhing Storm",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#The_Legendary_Desert.27s_Writhing_Storm",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Critical Chance",
								Type = "artifact",
								Value = 6
							},
							new DDOItemSetBonusProperty
							{
								Property = "Electric Spell Power",
								Type = "artifact",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 30
							}
						}
					}
				}
			});

			Sets.Add("The Desert's Starless Nights", new DDOItemSet
			{
				Name = "The Desert's Starless Nights",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#The_Desert.27s_Starless_Nights",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Critical Chance",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell Critical Chance",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage vs Helpless",
								Type = "artifact",
								Value = 5
							}
						}
					}
				}
			});

			Sets.Add("The Epic Desert's Starless Nights", new DDOItemSet
			{
				Name = "The Epic Desert's Starless Nights",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#The_Epic_Desert.27s_Starless_Nights",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Critical Chance",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell Critical Chance",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Power",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell Power",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage vs Helpless",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("The Legendary Desert's Starless Nights", new DDOItemSet
			{
				Name = "The Legendary Desert's Starless Nights",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#The_Legendary_Desert.27s_Starless_Nights",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Critical Chance",
								Type = "artifact",
								Value = 6
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell Critical Chance",
								Type = "artifact",
								Value = 6
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Spell Power",
								Type = "artifact",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Poison Spell Power",
								Type = "artifact",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage vs Helpless",
								Type = "artifact",
								Value = 15
							}
						}
					}
				}
			});

			Sets.Add("The Desert's Burning Sun", new DDOItemSet
			{
				Name = "The Desert's Burning Sun",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#The_Desert.27s_Burning_Sun",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Critical Chance",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 5
							}
						}
					}
				}
			});

			Sets.Add("The Epic Desert's Burning Sun", new DDOItemSet
			{
				Name = "The Epic Desert's Burning Sun",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#The_Epic_Desert.27s_Burning_Sun",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Critical Chance",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Power",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("The Legendary Desert's Burning Sun", new DDOItemSet
			{
				Name = "The Legendary Desert's Burning Sun",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#The_Legendary_Desert.27s_Burning_Sun",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Critical Chance",
								Type = "artifact",
								Value = 6
							},
							new DDOItemSetBonusProperty
							{
								Property = "Fire Spell Power",
								Type = "artifact",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 15
							}
						}
					}
				}
			});

			Sets.Add("Windlasher's Ferocity", new DDOItemSet
			{
				Name = "Windlasher's Ferocity",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Windlasher.27s_Ferocity",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Doublestrike",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doubleshot",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 5
							}
						}
					}
				}
			});

			Sets.Add("Epic Windlasher's Ferocity", new DDOItemSet
			{
				Name = "Epic Windlasher's Ferocity",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Epic_Windlasher.27s_Ferocity",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Doublestrike",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doubleshot",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Legendary Windlasher's Ferocity", new DDOItemSet
			{
				Name = "Legendary Windlasher's Ferocity",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Windlasher.27s_Ferocity",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Doublestrike",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doubleshot",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 15
							}
						}
					}
				}
			});

			Sets.Add("Menechtarun Scavenger", new DDOItemSet
			{
				Name = "Menechtarun Scavenger",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Menechtarun_Scavenger",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Healing Amplification",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Healing Amplification",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Healing Amplification",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Epic Menechtarun Scavenger", new DDOItemSet
			{
				Name = "Epic Menechtarun Scavenger",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Epic_Menechtarun_Scavenger",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Healing Amplification",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Healing Amplification",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Healing Amplification",
								Type = "artifact",
								Value = 20
							}
						}
					}
				}
			});

			Sets.Add("Legendary Menechtarun Scavenger", new DDOItemSet
			{
				Name = "Legendary Menechtarun Scavenger",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Menechtarun_Scavenger",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Healing Amplification",
								Type = "artifact",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Healing Amplification",
								Type = "artifact",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Healing Amplification",
								Type = "artifact",
								Value = 30
							}
						}
					}
				}
			});

			Sets.Add("The Desert's Biting Sands", new DDOItemSet
			{
				Name = "The Desert's Biting Sands",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#The_Desert.27s_Biting_Sands",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Critical Chance",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating Cap",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("The Epic Desert's Biting Sands", new DDOItemSet
			{
				Name = "The Epic Desert's Biting Sands",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#The_Epic_Desert.27s_Biting_Sands",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Critical Chance",
								Type = "artifact",
								Value = 4
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Power",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating Cap",
								Type = "artifact",
								Value = 20
							}
						}
					}
				}
			});

			Sets.Add("The Legendary Desert's Biting Sands", new DDOItemSet
			{
				Name = "The Legendary Desert's Biting Sands",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#The_Legendary_Desert.27s_Biting_Sands",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Critical Chance",
								Type = "artifact",
								Value = 6
							},
							new DDOItemSetBonusProperty
							{
								Property = "Force Spell Power",
								Type = "artifact",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating Cap",
								Type = "artifact",
								Value = 30
							}
						}
					}
				}
			});
			#endregion

			#region Saltmarsh sets
			Sets.Add("Saltmarsh Explorer", new DDOItemSet
			{
				Name = "Saltmarsh Explorer",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Saltmarsh_Explorer",
				SetBonuses = new List<DDOItemSetBonus>
					{
						new DDOItemSetBonus
						{
							MinimumItems = 5,
							Bonuses = new List<DDOItemSetBonusProperty>
							{
								new DDOItemSetBonusProperty
								{
									Property = "Physical Resistance Rating",
									Type = "artifact",
									Value = 5
								},
								new DDOItemSetBonusProperty
								{
									Property = "Magical Resistance Rating",
									Type = "artifact",
									Value = 5
								},
								new DDOItemSetBonusProperty
								{
									Property = "Strength",
									Type = "artifact",
									Value = 1
								},
								new DDOItemSetBonusProperty
								{
									Property = "Dexterity",
									Type = "artifact",
									Value = 1
								},
								new DDOItemSetBonusProperty
								{
									Property = "Constitution",
									Type = "artifact",
									Value = 1
								},
								new DDOItemSetBonusProperty
								{
									Property = "Intelligence",
									Type = "artifact",
									Value = 1
								},
								new DDOItemSetBonusProperty
								{
									Property = "Wisdom",
									Type = "artifact",
									Value = 1
								},
								new DDOItemSetBonusProperty
								{
									Property = "Charisma",
									Type = "artifact",
									Value = 1
								}
							}
						}
					}
			});

			Sets.Add("Epic Saltmarsh Explorer", new DDOItemSet
			{
				Name = "Epic Saltmarsh Explorer",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Epic_Saltmarsh_Explorer",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 5,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Universal Spell Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Strength",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Dexterity",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Constitution",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Intelligence",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Wisdom",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Charisma",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Healing Amplification",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Healing Amplification",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Healing Amplification",
								Type = "artifact",
								Value = 20
							}
						}
					}
				}
			});

			Sets.Add("Legendary Saltmarsh Explorer", new DDOItemSet
			{
				Name = "Legendary Saltmarsh Explorer",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Saltmarsh_Explorer",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 5,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating",
								Type = "artifact",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Universal Spell Power",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Strength",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Dexterity",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Constitution",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Intelligence",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Wisdom",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Charisma",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Positive Healing Amplification",
								Type = "artifact",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Negative Healing Amplification",
								Type = "artifact",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Repair Healing Amplification",
								Type = "artifact",
								Value = 30
							}
						}
					}
				}
			});
			#endregion

			#region Vault of Night sets
			Sets.Add("Mroranon's Might", new DDOItemSet
			{
				Name = "Mroranon's Might",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Mroranon.27s_Might",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Strength",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 5
							}
						}
					}
				}
			});

			Sets.Add("Epic Mroranon's Might", new DDOItemSet
			{
				Name = "Epic Mroranon's Might",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Epic_Mroranon.27s_Might",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Strength",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Legendary Mroranon's Might", new DDOItemSet
			{
				Name = "Legendary Mroranon's Might",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Mroranon.27s_Might",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Strength",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 15
							}
						}
					}
				}
			});

			Sets.Add("Silver Concord's Subtlety", new DDOItemSet
			{
				Name = "Silver Concord's Subtlety",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Silver_Concord.27s_Subtlety",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Fortification Bypass",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 5
							}
						}
					}
				}
			});

			Sets.Add("Epic Silver Concord's Subtlety", new DDOItemSet
			{
				Name = "Epic Silver Concord's Subtlety",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Epic_Silver_Concord.27s_Subtlety",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Fortification Bypass",
								Type = "artifact",
								Value = 20
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Legendary Silver Concord's Subtlety", new DDOItemSet
			{
				Name = "Legendary Silver Concord's Subtlety",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Silver_Concord.27s_Subtlety",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Fortification Bypass",
								Type = "artifact",
								Value = 30
							},
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 15
							}
						}
					}
				}
			});

			Sets.Add("Draconic Prophecy", new DDOItemSet
			{
				Name = "Draconic Prophecy",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Draconic_Prophecy",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Universal Spell Power",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating Cap",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Epic Draconic Prophecy", new DDOItemSet
			{
				Name = "Epic Draconic Prophecy",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Epic_Draconic_Prophecy",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Universal Spell Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating Cap",
								Type = "artifact",
								Value = 20
							}
						}
					}
				}
			});

			Sets.Add("Legendary Draconic Prophecy", new DDOItemSet
			{
				Name = "Legendary Draconic Prophecy",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Draconic_Prophecy",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Universal Spell Power",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Magical Resistance Rating Cap",
								Type = "artifact",
								Value = 30
							}
						}
					}
				}
			});

			Sets.Add("Kundarak Delving Equipment", new DDOItemSet
			{
				Name = "Kundarak Delving Equipment",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Kundarak_Delving_Equipment",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Dice",
								Type = "artifact",
								Value = 1
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage vs Helpless",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doublestrike",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doubleshot",
								Type = "artifact",
								Value = 5
							}
						}
					}
				}
			});

			Sets.Add("Epic Kundarak Delving Equipment", new DDOItemSet
			{
				Name = "Epic Kundarak Delving Equipment",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Epic_Kundarak_Delving_Equipment",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Dice",
								Type = "artifact",
								Value = 2
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage vs Helpless",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doublestrike",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doubleshot",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Legendary Kundarak Delving Equipment", new DDOItemSet
			{
				Name = "Legendary Kundarak Delving Equipment",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Kundarak_Delving_Equipment",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Sneak Attack Dice",
								Type = "artifact",
								Value = 3
							},
							new DDOItemSetBonusProperty
							{
								Property = "Damage vs Helpless",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doublestrike",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Doubleshot",
								Type = "artifact",
								Value = 15
							}
						}
					}
				}
			});

			Sets.Add("Wards of House Kundarak", new DDOItemSet
			{
				Name = "Wards of House Kundarak",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Wards_of_House_Kundarak",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class %",
								Type = "artifact",
								Value = 5
							},
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 10
							}
						}
					}
				}
			});

			Sets.Add("Epic Wards of House Kundarak", new DDOItemSet
			{
				Name = "Epic Wards of House Kundarak",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Epic_Wards_of_House_Kundarak",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class %",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 20
							}
						}
					}
				}
			});

			Sets.Add("Legendary Wards of House Kundarak", new DDOItemSet
			{
				Name = "Legendary Wards of House Kundarak",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Wards_of_House_Kundarak",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Armor Class %",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 30
							}
						}
					}
				}
			});

			Sets.Add("Epic Soul of the Red Dragon", new DDOItemSet
			{
				Name = "Epic Soul of the Red Dragon",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Epic_Soul_of_the_Red_Dragon",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 10
							},
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 20
							}
						}
					}
				}
			});

			Sets.Add("Legendary Soul of the Red Dragon", new DDOItemSet
			{
				Name = "Legendary Soul of the Red Dragon",
				WikiURL = "https://ddowiki.com/page/Named_item_sets#Legendary_Soul_of_the_Red_Dragon",
				SetBonuses = new List<DDOItemSetBonus>
				{
					new DDOItemSetBonus
					{
						MinimumItems = 2,
						Bonuses = new List<DDOItemSetBonusProperty>
						{
							new DDOItemSetBonusProperty
							{
								Property = "Melee Power",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Ranged Power",
								Type = "artifact",
								Value = 15
							},
							new DDOItemSetBonusProperty
							{
								Property = "Physical Resistance Rating",
								Type = "artifact",
								Value = 30
							}
						}
					}
				}
			});
			#endregion

			/*
				Sets.Add("", new DDOItemSet
				{
					Name = "",
					WikiURL = "",
					SetBonuses = new List<DDOItemSetBonus>
					{
						new DDOItemSetBonus
						{
							MinimumItems = 2,
							Bonuses = new List<DDOItemSetBonusProperty>
							{
								new DDOItemSetBonusProperty
								{
								}
							}
						}
					}
				});
			*/
		}

		public void AddItemProperty(string prop, string type, DDOItemData item)
		{
			DDOItemProperty ip;
			if (ItemProperties.ContainsKey(prop)) ip = ItemProperties[prop];
			else
			{
				ip = new DDOItemProperty { Property = prop };
				ItemProperties[prop] = ip;
			}

			if (!string.IsNullOrWhiteSpace(type) && !ip.Types.Contains(type)) ip.Types.Add(type);
			else if (string.IsNullOrWhiteSpace(type) && !ip.Types.Contains("")) ip.Types.Add("");

			if (item != null)
			{
				if (ip.Items.Find(i => i.Name == item.Name) == null) ip.Items.Add(item);

				// property hasn't seen this slot yet
				if ((ip.SlotsFoundOn & item.Slot) == 0)
				{
					// property has seen one slot
					if (Enum.IsDefined(typeof(SlotType), ip.SlotsFoundOn) && ip.SlotsFoundOn != SlotType.None)
					{
						if (SlotExclusiveItemProperties.ContainsKey(SlotType.None)) SlotExclusiveItemProperties[SlotType.None].Add(ip);
						else SlotExclusiveItemProperties[SlotType.None] = new List<DDOItemProperty> { ip };
					}

					if (SlotExclusiveItemProperties.ContainsKey(item.Slot)) SlotExclusiveItemProperties[item.Slot].Add(ip);
					else SlotExclusiveItemProperties[item.Slot] = new List<DDOItemProperty> { ip };

					ip.SlotsFoundOn |= item.Slot;
				}
			}
		}

		public string AddItem(DDOItemData item)
		{
			// add to the slot
			Slots[item.Slot].Items.Add(item);
			// go through all item properties, to include optional ones
			foreach (var ip in item.Properties)
			{
				if (ip.Options != null && !ip.HideOptions)
				{
					foreach (var o in ip.Options)
					{
						if (o.Type == "set")
						{
							try { Sets[o.Property].Items.Add(item); }
							catch { return "- " + item.Name + " referenced bad set " + o.Property; }
						}
						else AddItemProperty(o.Property, o.Type, item);
					}
				}
				else if (ip.Type == "set")
				{
					try { Sets[ip.Property].Items.Add(item); }
					catch { return "- " + item.Name + " referenced bad set " + ip.Property; }
				}
				else AddItemProperty(ip.Property, ip.Type, item);
			}

			Items.Add(item);

			return null;
		}
	}
}
