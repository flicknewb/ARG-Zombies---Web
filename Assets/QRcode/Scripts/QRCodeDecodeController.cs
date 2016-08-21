﻿/// <summary>
/// write by 52cwalk,if you have some question ,please contract lycwalk@gmail.com
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using System.IO;

public class QRCodeDecodeController : MonoBehaviour
{
	public delegate void QRScanFinished(string str);  //declare a delegate to deal with the QRcode decode complete
	public event QRScanFinished onQRScanFinished;  		//declare a event with the delegate to trigger the complete event
	
	bool decoding = false;		
	bool tempDecodeing = false;
	string dataText = null;
	public DeviceCameraController e_DeviceController = null; 
	private Color[] orginalc;   	//the colors of the camera data.
	private Color32[] targetColorARR;   	//the colors of the camera data.
	private byte[] targetbyte;		//the pixels of the camera image.
	private int W, H, WxH;			//width/height of the camera image
	int byteIndex = 0;				
	int framerate = 0; 		

	int blockWidth = 350;

	BarcodeReader barReader;
	void Start()
	{
		barReader = new BarcodeReader ();
		barReader.AutoRotate = true;
		barReader.TryInverted = true;
		
		if (!e_DeviceController) {
			e_DeviceController = GameObject.FindObjectOfType<DeviceCameraController>();
			if(!e_DeviceController)
			{
				Debug.LogError("the Device Controller is not exsit,Please Drag DeviceCamera from project to Hierarchy");
			}
		}

		targetColorARR = new Color32[blockWidth * blockWidth];
	}
	
	void Update()
	{
		if (framerate++ % 4 == 0) {
			if (!e_DeviceController.isPlaying  ) {
				return;
			}
			
			if (e_DeviceController.isPlaying && !decoding && e_DeviceController.cameraTexture.isPlaying)
			{

				W = e_DeviceController.cameraTexture.width;					// get the image width
				H = e_DeviceController.cameraTexture.height;				// get the image height 

				int posx = ((W-blockWidth)>>1);//
				int posy = ((H-blockWidth)>>1);
				
				orginalc = e_DeviceController.cameraTexture.GetPixels(posx,posy,blockWidth,blockWidth);// get the webcam image colors
				
                //convert the color(float) to color32 (byte)
				for(int i=0;i!= blockWidth;i++)
				{
					for(int j = 0;j!=blockWidth ;j++)
					{
						targetColorARR[i + j*blockWidth].r = (byte)( orginalc[i + j*blockWidth].r*255);
						targetColorARR[i + j*blockWidth].g = (byte)(orginalc[i + j*blockWidth].g*255);
						targetColorARR[i + j*blockWidth].b = (byte)(orginalc[i + j*blockWidth].b*255);
						targetColorARR[i + j*blockWidth].a = 1;
					}
				}

				// scan the qrcode 
				Loom.RunAsync(() =>
				              {
					try
					{
						Result data;
						data = barReader.Decode(targetColorARR,blockWidth,blockWidth);//start decode
						if (data != null) // if get the result success
						{
							decoding = true; 	// set the variable is true
							dataText = data.Text;	// use the variable to save the code result
						}

					}
					catch (Exception e)
					{
						decoding = false;
					}
				});	
			}
			
			if(decoding)
			{
				// if the status variable is change
				if(tempDecodeing != decoding)
				{
					onQRScanFinished(dataText);//triger the scan finished event;
				}
				tempDecodeing = decoding;
			}
		}
	}
	
	/// <summary>
	/// Reset this scan param
	/// </summary>
	public void Reset()
	{
		decoding = false;
		tempDecodeing = decoding;
	}
	
	/// <summary>
	/// Stops the work.
	/// </summary>
	public void StopWork()
	{
		decoding = true;
		if (e_DeviceController != null) {
			e_DeviceController.StopWork();
		}
	}
	
	/// <summary>
	/// Decodes the by static picture.
	/// </summary>
	/// <returns> return the decode result string </returns>
	/// <param name="tex">target texture.</param>
	public static string DecodeByStaticPic(Texture2D tex)
	{
		BarcodeReader codeReader = new BarcodeReader ();
		codeReader.AutoRotate = true;
		codeReader.TryInverted = true;
		
		Result data = codeReader.Decode (tex.GetPixels32 (), tex.width, tex.height);
		if (data != null) {
			return data.Text;
		} else {
			return "decode failed!";
		}
	}
	
}