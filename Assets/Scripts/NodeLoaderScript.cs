using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public class NodeLoaderScript : MonoBehaviour
{

    public List<NodeScript> nodescripts = new List<NodeScript>();

    // Start is called before the first frame update
    void Start()
    {
        dynamic js = JsonConvert.DeserializeObject(File.ReadAllText("Assets/Data/Sats.json"));
        foreach (var obj in js)
        {

            // Add new object to the list
            string[] OPS = obj.OPS.ToObject<string[]>(); // Explicit casting required
            // NodeScript ns = new NodeScript(NodeScript.CreateNode(transform.parent, (string)obj.Name, TLE));
            NodeScript ns = new NodeScript(transform.parent, (string)obj.Name, (string)obj.Type, OPS, (string)obj.IP, (int)obj.Port);
            nodescripts.Add(ns);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}