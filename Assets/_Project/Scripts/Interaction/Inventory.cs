using System.Collections.Generic;
using UnityEngine;
namespace TheLightHouse {
public sealed class Inventory : MonoBehaviour {
readonly HashSet<string> items=new();
void OnEnable()=>GameEvents.ItemCollected+=Collect;
void OnDisable()=>GameEvents.ItemCollected-=Collect;
void Collect(ItemCollectedEvent e)=>items.Add(e.Id);
public bool Consume(string id)=>items.Remove(id);
public bool Contains(string id)=>items.Contains(id);
}}
