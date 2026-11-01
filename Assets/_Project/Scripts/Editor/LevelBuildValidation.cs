using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

namespace TheLightHouse.Editor
{
    public static class LevelBuildValidation
    {
        static readonly string[][] BeatOrder=
        {
            new[]{"A1-B01","A1-B02","A1-B03","A1-B04","A1-B05","A1-B06"},
            new[]{"A2-B01","A2-B02","A2-B03","A2-B04","A2-B05","A2-B06","A2-B07"},
            new[]{"A3-B01","A3-B02","A3-B03","A3-B04","A3-B05","A3-B06","A3-B07","A3-B08","A3-B09"},
            new[]{"A4-B01","A4-B02","A4-B03","A4-B04","A4-B05","A4-B06"},
            new[]{"A5-B01","A5-B02","A5-B03","A5-B04","A5-B05","A5-B06"}
        };

        [MenuItem("TheLightHouse/Validate Full Lighthouse and HUD")]
        public static void ValidateBatch()
        {
            List<string> failures=new();
            for(int act=1;act<=5;act++)ValidateAct(act,failures);
            if(failures.Count>0)throw new InvalidOperationException("Level validation failed:\n"+string.Join("\n",failures));
            Debug.Log("Level validation passed: Acts 1–5 retain gameplay anchors, shared layout/HUD, and canonical beat order.");
        }

        static void ValidateAct(int act,List<string> failures)
        {
            string scenePath=$"Assets/_Project/Scenes/Act{act}.unity";EditorSceneManager.OpenScene(scenePath,OpenSceneMode.Single);
            Require<Door>(act,"Door",failures);Require<Radio>(act,"Radio",failures);Require<MealTable>(act,"MealTable",failures);Require<LanternInteract>(act,"LanternInteract",failures);Require<CrankStation>(act,"CrankStation",failures);Require<CellarHum>(act,"CellarHum",failures);Require<WetFootprints>(act,"WetFootprints",failures);
            RequireName(act,"Boat",failures);RequireName(act,"Lighthouse Layout",failures);RequireName(act,"HUD Root",failures);RequireName(act,"Pause Menu",failures);
            CountAtLeast<RoomTrigger>(act,10,"room triggers",failures);CountAtLeast<HidingSpot>(act,8,"hiding spots",failures);CountAtLeast<NavMeshLink>(act,8,"NavMesh links",failures);
            if(act>=2){Require<DrownedManager>(act,"figures manager",failures);CountAtLeast<DrownedFigure>(act,4,"figures",failures);}
            if(act>=4){Require<HiddenCellarRoom>(act,"hidden shelf",failures);RequireName(act,"hidden shelf",failures);Require<Ledger>(act,"Ledger",failures);Require<LockedRoomDoor>(act,"Crane's room door",failures);Require<FamilyPhoto>(act,"FamilyPhoto",failures);}
            if(act==5){Require<DoorwayBlock>(act,"DoorwayBlock",failures);Require<ShorelineEnding>(act,"ShorelineEnding",failures);}
            ValidateBeats(act,failures);ValidateBeatFiring(act,failures);ValidateMaterials(act,failures);
        }

        static void ValidateBeats(int act,List<string> failures)
        {
            StoryDirector director=Object.FindAnyObjectByType<StoryDirector>();if(!director){failures.Add($"Act{act}: StoryDirector missing");return;}
            SerializedObject serialized=new(director);List<string> ids=ReadBeats(serialized.FindProperty("beats")).Concat(ReadBeats(serialized.FindProperty("optionalBeats"))).Select(beat=>beat.id).ToList();
            string[] expected=BeatOrder[act-1];int cursor=0;foreach(string id in ids)if(cursor<expected.Length&&id==expected[cursor])cursor++;
            if(cursor!=expected.Length)failures.Add($"Act{act}: beat order expected {string.Join(", ",expected)}; found {string.Join(", ",ids)}");
        }

        static void ValidateBeatFiring(int act,List<string> failures)
        {
            StoryDirector director=Object.FindAnyObjectByType<StoryDirector>();if(!director)return;SerializedObject serialized=new(director);StoryBeat[] main=ReadBeats(serialized.FindProperty("beats"));StoryBeat[] optional=ReadBeats(serialized.FindProperty("optionalBeats"));
            Type type=typeof(StoryDirector);BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;type.GetField("next",flags)?.SetValue(director,0);(type.GetField("completed",flags)?.GetValue(director) as HashSet<string>)?.Clear();(type.GetField("pending",flags)?.GetValue(director) as HashSet<string>)?.Clear();
            List<string> fired=new();void Record(StoryBeatTriggeredEvent value)=>fired.Add(value.Id);GameEvents.StoryBeatTriggered+=Record;
            try
            {
                foreach(StoryBeat beat in main)InvokeTrigger(director,beat,type,flags);
                foreach(StoryBeat beat in optional)InvokeTrigger(director,beat,type,flags);
            }
            finally{GameEvents.StoryBeatTriggered-=Record;}
            string[] expected=BeatOrder[act-1];if(!expected.SequenceEqual(fired))failures.Add($"Act{act}: simulated beat firing expected {string.Join(", ",expected)}; fired {string.Join(", ",fired)}");
        }

        static void InvokeTrigger(StoryDirector director,StoryBeat beat,Type type,BindingFlags flags)
        {
            if(beat.trigger==StoryTrigger.Signal)type.GetMethod("Signal",flags)?.Invoke(director,new object[]{beat.triggerId});
            else if(beat.trigger==StoryTrigger.DoorOpened)type.GetMethod("Door",flags)?.Invoke(director,new object[]{new DoorStateChangedEvent(beat.triggerId,DoorState.Closed,DoorState.Open)});
            else if(beat.trigger==StoryTrigger.LampLit)type.GetMethod("Lamp",flags)?.Invoke(director,new object[]{new LampStateChangedEvent(LampState.Unlit,LampState.FuelLit,1f,true)});
        }

        static StoryBeat[] ReadBeats(SerializedProperty property)
        {
            if(property==null||!property.isArray)return Array.Empty<StoryBeat>();List<StoryBeat> beats=new();for(int i=0;i<property.arraySize;i++)if(property.GetArrayElementAtIndex(i).objectReferenceValue is StoryBeat beat)beats.Add(beat);return beats.ToArray();
        }

        static void ValidateMaterials(int act,List<string> failures)
        {
            foreach(Renderer renderer in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                if(renderer.sharedMaterial==null||renderer.sharedMaterial.shader==null)failures.Add($"Act{act}: {renderer.name} has a missing material/shader");
                if(renderer is ParticleSystemRenderer&&renderer.sharedMaterial&&renderer.sharedMaterial.shader&&!renderer.sharedMaterial.shader.name.Contains("Universal Render Pipeline"))failures.Add($"Act{act}: particle {renderer.name} is not using a URP shader");
            }
        }

        static void Require<T>(int act,string label,List<string> failures) where T:Object{if(Object.FindObjectsByType<T>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length==0)failures.Add($"Act{act}: {label} missing");}
        static void CountAtLeast<T>(int act,int minimum,string label,List<string> failures) where T:Object{int count=Object.FindObjectsByType<T>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length;if(count<minimum)failures.Add($"Act{act}: expected at least {minimum} {label}, found {count}");}
        static void RequireName(int act,string name,List<string> failures){if(!Resources.FindObjectsOfTypeAll<GameObject>().Any(value=>value.scene.isLoaded&&value.name==name))failures.Add($"Act{act}: GameObject '{name}' missing");}
    }
}
