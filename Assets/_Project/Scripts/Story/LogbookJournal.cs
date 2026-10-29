using System.Collections.Generic;
using UnityEngine;

namespace TheLightHouse
{
    public sealed class LogbookJournal : MonoBehaviour
    {
        [SerializeField] LogbookReaderUI reader;
        [SerializeField] LogbookPage[] allPages;
        readonly List<LogbookPage> pages = new();
        readonly HashSet<string> read = new();
        public IReadOnlyList<LogbookPage> Pages => pages;
        public bool HasRead(string id) => read.Contains(id);

        public void CollectAndRead(LogbookPage page)
        {
            if (!page || (page.lockedUntilPhoto && !HasRead("crane-photo-unlock"))) return;
            if (!pages.Contains(page))
            {
                pages.Add(page);
                GameEvents.RaiseItemCollected(new ItemCollectedEvent(page.id));
            }
            read.Add(page.id);
            GameEvents.RaisePageRead(new PageReadEvent(page.id, page.storyFlag));
            reader?.Show(page.title, page.body);
        }

        public void UnlockFinalEntry() => read.Add("crane-photo-unlock");
        public void OpenPage(LogbookPage page) { if (page && pages.Contains(page)) reader?.Show(page.title, page.body); }
        public string[] PageIds(){var ids=new string[pages.Count];for(int i=0;i<pages.Count;i++)ids[i]=pages[i].id;return ids;}
        public void Restore(IEnumerable<string> ids)
        {
            if(ids==null||allPages==null)return;
            foreach(var id in ids)foreach(var page in allPages)if(page&&page.id==id&&!pages.Contains(page)){pages.Add(page);read.Add(id);}
        }
        public void GiveAllPages(){if(allPages==null)return;foreach(var page in allPages)if(page&&!pages.Contains(page)){pages.Add(page);read.Add(page.id);GameEvents.RaiseItemCollected(new(page.id));}}
    }
}
