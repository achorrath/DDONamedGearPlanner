﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DDONamedGearPlanner
{
	public class BuildFilter
	{
		// this will be SlotType.None for gear set filters
		public SlotType Slot;
		public string Property;
		public string Type;
		public bool Include;
	}

	public class BuildItem
	{
		// this is for UI bookkeeping
		public bool InUse;
		public DDOItemData Item;
		// this is how we simulate property options being set
		public List<ItemProperty> OptionProperties = new List<ItemProperty>();

		public BuildItem(DDOItemData item)
		{
			Item = item;
		}
	}

	public class GearSetProperty
	{
		public bool IsGroup;
		public string Property;
		public float TotalValue;
		public List<ItemProperty> ItemProperties = new List<ItemProperty>();

		// this adds an item property, sorting by highest value and type alphabetically
		public void AddItemProperty(ItemProperty ip)
		{
			int place = -1;
			for (int i = 0; i < ItemProperties.Count; i++)
			{
				// no type means sort only by value
				if (string.IsNullOrWhiteSpace(ip.Type))
				{
					if (ItemProperties[i].Value >= ip.Value) continue;
					place = i;
					break;
				}
				// any type should appear before no type
				if (string.IsNullOrWhiteSpace(ItemProperties[i].Type))
				{
					place = i;
					break;
				}
				// alphabetical listing of types please
				int sc = string.Compare(ItemProperties[i].Type, ip.Type);
				if (sc < 0) continue;
				// in order by value next
				else if (sc == 0)
				{
					if (ItemProperties[i].Value > ip.Value) continue;
					else
					{
						place = i;
						break;
					}
				}
				else
				{
					place = i;
					break;
				}
			}
			if (place == -1) ItemProperties.Add(ip);
			else ItemProperties.Insert(place, ip);
		}
	}

	public class GearSet
	{
		public List<BuildItem> Items = new List<BuildItem>();
		public List<GearSetProperty> Properties = new List<GearSetProperty>();
		public float Rating;
		public float Penalty;

		void AddProperties(List<ItemProperty> properties)
		{
			foreach (var p in properties)
			{
				GearSetProperty gsp = Properties.Find(pr => pr.Property == p.Property);
				if (gsp == null)
				{
					// options get ignored, as the optionals will be applied separately
					if (p.Options != null && p.Options.Count > 0) continue;
					// we never want to show minimum level or handedness
					if (p.Property == "Minimum Level" || p.Property == "Handedness") continue;

					gsp = new GearSetProperty { Property = p.Property };
					if (gsp.Property == "Damage Reduction") gsp.IsGroup = true;
					else if (gsp.Property == "Augment Slot") gsp.IsGroup = true;
					else if (p.Type == "set") gsp.IsGroup = true;
					Properties.Add(gsp);
				}

				gsp.AddItemProperty(p);
			}
		}

		public void AddItem(BuildItem item)
		{
			Items.Add(item);
			AddProperties(item.Item.Properties);
			if (item.OptionProperties != null) AddProperties(item.OptionProperties);
		}

		void CalculatePropertyGroups()
		{
			foreach (var gsp in Properties)
			{
				if (gsp.IsGroup) continue;

				gsp.TotalValue = 0;
				string lasttype = null;
				foreach (var ip in gsp.ItemProperties)
				{
					if (string.IsNullOrWhiteSpace(lasttype) || lasttype != ip.Type)
					{
						gsp.TotalValue += ip.Value;
						lasttype = ip.Type;
					}
				}
			}
		}

		public void ProcessItems(DDODataset dataset)
		{
			// need to find all set properties with qualifying set bonuses and expand them into the item properties
			for (int i = 0; i < Properties.Count; i++)
			{
				var gsp = Properties[i];
				if (gsp.IsGroup && gsp.ItemProperties[0].Type == "set")
				{
					List<ItemProperty> ips = new List<ItemProperty>();
					DDOItemSet set = dataset.Sets[gsp.ItemProperties[0].Property];
					DDOItemSetBonus sb = null;
					foreach (var sbs in set.SetBonuses)
					{
						if (sbs.MinimumItems > gsp.ItemProperties.Count) break;
						sb = sbs;
					}

					if (sb == null) continue;

					foreach (var sip in sb.Bonuses)
					{
						ItemProperty ip = new ItemProperty { Property = sip.Property, Type = sip.Type, Value = sip.Value };
						ip.SetBonusOwner = set.Name;
						ips.Add(ip);
					}

					if (ips.Count > 0) AddProperties(ips);
				}
			}

			CalculatePropertyGroups();
		}
	}

    public class GearSetBuild
    {
		public Dictionary<SlotType, List<BuildFilter>> Filters = new Dictionary<SlotType, List<BuildFilter>>();
		// so we can remember the lock state of the slots
		public List<EquipmentSlotType> LockedSlots = new List<EquipmentSlotType>();
    }
}