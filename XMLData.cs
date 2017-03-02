﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;

namespace ServerSideCharacter
{
	public class XMLData
	{
		public Dictionary<string, ServerPlayer> Data = new Dictionary<string, ServerPlayer>();

		public XMLData()
		{

		}

		private string ReadNext(XmlNodeList info, ref int i)
		{
			return info.Item(i++).InnerText;
		}


		private void TryReadItemInfo(Dictionary<string, Mod> modTable, XmlNodeList info, 
			ServerPlayer player, ref int i, int id, ref Item[] slots)
		{
			int type = 0;
			string text = ReadNext(info, ref i);
			//如果是mod物品
			if (text[0] == '$')
			{
				text = text.Substring(1);

				string modName = text.Substring(0, text.IndexOf('.'));
				string itemName = text.Substring(text.LastIndexOf('.') + 1);
				//解析物品id，字典中有mod名字
				if (modTable.ContainsKey(modName))
				{
					type = modTable[modName].ItemType(itemName);
					//如果数据合法
					if (type > 0)
					{
						slots[id].netDefaults(type);
					}
					else
					{
						slots[id].netDefaults(ModLoader.GetMod("ModLoader").ItemType("MysteryItem"));
						//物品数据会丢失
					}
				}
				else
				{
					slots[id].netDefaults(ModLoader.GetMod("ModLoader").ItemType("MysteryItem"));
					//物品数据会丢失

				}
			}
			else
			{
				type = Convert.ToInt32(text);

				if (type != 0)
				{
					slots[id].netDefaults(type);
				}
			}
		}

		public XMLData(string path)
		{
			if (File.Exists(path))
			{
				XmlReaderSettings settings = new XmlReaderSettings();
				settings.IgnoreComments = true;                         //忽略文档里面的注释
				XmlDocument xmlDoc = new XmlDocument();
				XmlReader reader = XmlReader.Create(path, settings);
				xmlDoc.Load(reader);
				XmlNode xn = xmlDoc.SelectSingleNode("Players");
				var list = xn.ChildNodes;
				Dictionary<string, Mod> modTable = new Dictionary<string, Mod>();
				foreach(var mod in ModLoader.LoadedMods)
				{
					modTable.Add(mod.Name, mod);
				}
				foreach (var node in list)
				{
					XmlElement pData = (XmlElement)node;
					ServerPlayer player = new ServerPlayer();
					var info = pData.ChildNodes;
					int i = 0;
					player.Name = pData.GetAttribute("name");
					player.Hash = pData.GetAttribute("hash");
					player.HasPassword = Convert.ToBoolean(ReadNext(info, ref i));
					player.Password = ReadNext(info, ref i);
					player.LifeMax = Convert.ToInt32(ReadNext(info, ref i));
					player.StatLife = Convert.ToInt32(ReadNext(info, ref i));
					player.ManaMax = Convert.ToInt32(ReadNext(info, ref i));
					player.StatMana = Convert.ToInt32(ReadNext(info, ref i));
					for (int id = 0; id < player.inventroy.Length; id++)
					{
						TryReadItemInfo(modTable, info, player, ref i, id, ref player.inventroy);
						player.inventroy[id].Prefix(Convert.ToByte((info.Item(i - 1) as XmlElement).GetAttribute("prefix")));
						player.inventroy[id].stack =
							Convert.ToInt32((info.Item(i - 1) as XmlElement).GetAttribute("stack"));
					}
					for (int id = 0; id < player.armor.Length; id++)
					{
						int type = Convert.ToInt32(ReadNext(info, ref i));
						if (type != 0)
						{
							player.armor[id].SetDefaults(type);
							player.armor[id].Prefix(Convert.ToByte((info.Item(i - 1) as XmlElement).GetAttribute("prefix")));
						}
					}
					for (int id = 0; id < player.dye.Length; id++)
					{
						int type = Convert.ToInt32(ReadNext(info, ref i));
						if (type != 0)
						{
							player.dye[id].SetDefaults(type);
						}
					}
					for (int id = 0; id < player.miscEquips.Length; id++)
					{
						int type = Convert.ToInt32(ReadNext(info, ref i));
						if (type != 0)
						{
							player.miscEquips[id].SetDefaults(type);
						}
					}
					for (int id = 0; id < player.miscDye.Length; id++)
					{
						int type = Convert.ToInt32(ReadNext(info, ref i));
						if (type != 0)
						{
							player.miscDye[id].SetDefaults(type);
						}
					}
					for (int id = 0; id < player.bank.item.Length; id++)
					{
						int type = Convert.ToInt32(ReadNext(info, ref i));
						if (type != 0)
						{
							player.bank.item[id].SetDefaults(type);
							player.bank.item[id].Prefix(Convert.ToByte((info.Item(i - 1) as XmlElement).GetAttribute("prefix")));
							player.bank.item[id].stack =
								Convert.ToInt32((info.Item(i - 1) as XmlElement).GetAttribute("stack"));
						}
					}
					for (int id = 0; id < player.bank2.item.Length; id++)
					{
						int type = Convert.ToInt32(ReadNext(info, ref i));
						if (type != 0)
						{
							player.bank2.item[id].SetDefaults(type);
							player.bank2.item[id].Prefix(Convert.ToByte((info.Item(i - 1) as XmlElement).GetAttribute("prefix")));
							player.bank2.item[id].stack =
								Convert.ToInt32((info.Item(i - 1) as XmlElement).GetAttribute("stack"));
						}
					}
					for (int id = 0; id < player.bank3.item.Length; id++)
					{
						int type = Convert.ToInt32(ReadNext(info, ref i));
						if (type != 0)
						{
							player.bank3.item[id].SetDefaults(type);
							player.bank3.item[id].Prefix(Convert.ToByte((info.Item(i - 1) as XmlElement).GetAttribute("prefix")));
							player.bank3.item[id].stack =
								Convert.ToInt32((info.Item(i - 1) as XmlElement).GetAttribute("stack"));
						}
					}

					Data.Add(player.Name, player);
				}
				ServerSideCharacter.MainWriter = new XMLWriter(path);
				reader.Close();
			}
			else
			{
				XMLWriter writer = new XMLWriter(path);
				writer.Create();
				ServerSideCharacter.MainWriter = writer;
			}
		}
	}
}
