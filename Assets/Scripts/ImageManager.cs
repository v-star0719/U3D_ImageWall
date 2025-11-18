using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageManager : MonoBehaviour
{
	public static ImageManager Instance;

	public List<ImageObject> SelectedImageObjects = new List<ImageObject>();
	public List<ImageObject> ImageObjects = new List<ImageObject>();

	void Awake()
	{
		Instance = this;
	}

	// Use this for initialization
	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
		


	}

	public void AddSelect(ImageObject o)
	{
		SelectedImageObjects.Add(o);
	}

	public void Add(ImageObject o)
	{
		ImageObjects.Add(o);
	}
}
