using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class EventService : MonoBehaviour
{
    public string serverUrl = "https://echo.zuplo.io/";
    [SerializeField] private float cooldownBeforeSend = 1.0f;

    private TracksHolder holder = new();
    private TracksRepository repository = new();


    private float cooldown;

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        holder = repository.Load();
        if (holder.HasEvents)
            StartCoroutine(SendEvents());
    }


    public void TrackEvent(string type, string data)
    {
        holder.AddEvent(type, data);
        repository.Save(holder);
        StartCoroutine(SendEvents());
    }

    private IEnumerator SendEvents()
    {
        if (cooldown > 0)
            yield break;

        cooldown = cooldownBeforeSend;
        while (cooldown > 0)
        {
            cooldown -= Time.deltaTime;
            yield return null;
        }

        if (!holder.HasEvents)
            yield break;

        var sendHolder = new TracksHolder();
        foreach (var item in holder.events)
            sendHolder.AddEvent(item.type, item.type);

        using UnityWebRequest webRequest = UnityWebRequest.Post(serverUrl, sendHolder.ToJson());
        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            StartCoroutine(SendEvents());
        }
        else
        {
            long responseCode = webRequest.responseCode;

            if (responseCode != 200)
            {
                StartCoroutine(SendEvents());
            }

            Debug.Log($"Send events: {sendHolder.ToJson()}");
            holder.RemoveItems(sendHolder.events.Count);
            repository.Save(holder);
        }
    }


    private class TracksRepository
    {
        private const string TracksRepositoryKey = nameof(TracksRepositoryKey);

        public TracksHolder Load()
        {
            if (!PlayerPrefs.HasKey(TracksRepositoryKey)) return new TracksHolder();

            var data = PlayerPrefs.GetString(TracksRepositoryKey);
            return JsonUtility.FromJson<TracksHolder>(data);
        }

        public void Save(TracksHolder holder)
        {
            var data = holder.ToJson();
            PlayerPrefs.SetString(TracksRepositoryKey, data);
            PlayerPrefs.Save();
        }
    }

    [Serializable]
    private class TracksHolder
    {
        public bool HasEvents => events.Count > 0;
        public List<TrackItem> events = new();

        public void AddEvent(string type, string data)
        {
            events.Add(new TrackItem {data = data, type = type});
        }

        public string ToJson() => JsonUtility.ToJson(this);

        public void RemoveItems(int eventsCount)
        {
            if (events.Count >= eventsCount)
                events.RemoveRange(0, eventsCount);
        }
    }

    [Serializable]
    private class TrackItem
    {
        public string type;
        public string data;
    }
}