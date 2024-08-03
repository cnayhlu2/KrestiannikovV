using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    public class EventServiceTester : MonoBehaviour
    {
        private const float firstMessage = 0f;

        [SerializeField] private EventService eventService;

        private float cooldown;


        private void Start()
        {
            cooldown = 0;
        }

        private void Update()
        {
            if (cooldown < 0)
            {
                cooldown = Random.Range(0, 3f);
                eventService.TrackEvent(GenerateRandomString(5), GenerateRandomString(10));
            }

            cooldown -= Time.deltaTime;
        }

        private string GenerateRandomString(int length)
        {
            System.Random random = new System.Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}