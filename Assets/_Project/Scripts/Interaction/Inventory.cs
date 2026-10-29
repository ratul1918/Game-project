using System.Collections.Generic;
using UnityEngine;
namespace TheLightHouse {
public sealed class Inventory : MonoBehaviour {
readonly HashSet<string> items=new();
public IEnumerable<string> Items=>items;
void OnEnable()=>GameEvents.ItemCollected+=Collect;
void OnDisable()=>GameEvents.ItemCollected-=Collect;
void Collect(ItemCollectedEvent e)=>items.Add(e.Id);
public bool Consume(string id)=>items.Remove(id);
public bool Contains(string id)=>items.Contains(id);
public void Restore(IEnumerable<string> ids){items.Clear();if(ids==null)return;foreach(var id in ids)items.Add(id);}
}}
