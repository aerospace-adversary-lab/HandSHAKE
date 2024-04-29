using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeptomoby.OrbitTools;
using System;

using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;

public class NodeScript : MonoBehaviour
{
    public DateTime simTime;

    public string name;
    public string type;
    public string[] orbitParams;

    public string ip;
    
    public int port;

    private static GameObject nodeGameObj;

    private TcpClient socketConnection;     
    private Thread clientReceiveThread; 


    // Constructor
    // Possible alternative: https://discussions.unity.com/t/passing-values-into-base-constructor-with-monobehaviours/27484/2
    public NodeScript(Transform parent, string name, string type, string[] ops, string ip, int port)
    {
        nodeGameObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var thisObj = nodeGameObj.AddComponent<NodeScript>();
            //calls Start() on the object and initializes it.
            thisObj.transform.parent = parent;
            thisObj.name = name;
            thisObj.type = type;
            thisObj.orbitParams = ops;
            thisObj.ip = ip;
            thisObj.port = port;
    }

    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.localScale = new Vector3(200,200,200);

        if (ip != null)
        {
            ConnectToTcpServer("open"); 
            gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.yellow);
        }
    }

    // Update is called once per frame
    void Update()
    {

         // Get updated simulation time
         simTime = GetComponentInParent<SimHandlerScript>().simTime;

         if(type=="Sat")
         {
            // Derive ECI coordinates from TLE
            var tle = new TwoLineElements(orbitParams[0], orbitParams[1], orbitParams[2]);
            Satellite sat = new Satellite(tle);
            Eci eci = sat.PositionEci(simTime);

            // Set sim coordinates according to ECI
            gameObject.transform.position = new Vector3((float)eci.Position.X, (float)eci.Position.Z, (float)eci.Position.Y);
         }
         if(type=="Ground")
         {
            gameObject.transform.position = latLonToEcef(Convert.ToDouble(orbitParams[1]), Convert.ToDouble(orbitParams[2]));
         }
    }


    // Converts latitude, longitude to ECEF coordinate system
    Vector3 latLonToEcef(double lat, double lon, double alt = 0)
    {
        double WGS84_A = 6378137.0;
        double WGS84_E = 0.081819190842622;

        double clat = Math.Cos(Math.PI * lat / 180);
        double slat = Math.Sin(Math.PI * lat / 180);
        double clon = Math.Cos(Math.PI * lon / 180);
        double slon = Math.Sin(Math.PI * lon / 180);

        double N = WGS84_A / Math.Sqrt(1.0 - WGS84_E * WGS84_E * slat * slat);

        double x = ((N + alt) * clat * clon)/1000;
        double y = ((N + alt) * clat * slon)/1000;
        double z = ((N * (1.0 - WGS84_E * WGS84_E) + alt) * slat)/1000;

        return new Vector3((float)x, (float)z, (float)y);
    }

    private void ConnectToTcpServer (string state) 
    {        
        if (state == "close") {             
            clientReceiveThread.Abort();    
        }       
        else if (state == "open") {
            clientReceiveThread = new Thread (new ThreadStart(ListenForData));          
            clientReceiveThread.IsBackground = true;            
            clientReceiveThread.Start(); 
        }   
        else {
            return;
        } 
    }   

    void ListenForData() 
    {      
        try {           
            socketConnection = new TcpClient("127.0.0.1", port);   
            print("Connected to "+name+" on port "+port);   
                  
            Byte[] bytes = new Byte[1024];             
            while (true) {              
                // Get a stream object for reading              
                using (NetworkStream stream = socketConnection.GetStream()) {                   
                    int length;                     
                    // Read incomming stream into byte array.                   
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) {                       
                        var incommingData = new byte[length];                       
                        Array.Copy(bytes, 0, incommingData, 0, length);                         
                        // Convert byte array to string message.                        
                        string serverMessage = Encoding.ASCII.GetString(incommingData);                         
                        Debug.Log("Interaction from " +name + ": " + serverMessage);                  
                    }               
                }           
            }         
        }         
        catch (SocketException socketException) {             
            Debug.Log("Socket exception: " + socketException);         
        }     
    }   

}