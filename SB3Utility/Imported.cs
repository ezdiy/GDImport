using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using System.Numerics;
using Matrix = System.Numerics.Matrix4x4;
using Color4 = System.Numerics.Vector4;

namespace SB3Utility
{
	public interface IImported
	{
		List<ImportedFrame> FrameList { get; }
		List<ImportedMesh> MeshList { get; }
		List<ImportedMaterial> MaterialList { get; }
		List<ImportedTexture> TextureList { get; }
		List<ImportedAnimation> AnimationList { get; }
		List<ImportedMorph> MorphList { get; }
	}

	public class ImportedFrame : ObjChildren<ImportedFrame>, IObjChild
	{
		public string Name { get; set; }
		public Matrix Matrix { get; set; }

		public dynamic Parent { get; set; }
	}

	public class ImportedMesh
	{
		public string Name { get; set; }
		public List<ImportedSubmesh> SubmeshList { get; set; }
		public List<ImportedBone> BoneList { get; set; }
	}

	public class ImportedSubmesh
	{
		public List<ImportedVertex> VertexList { get; set; }
		public List<ImportedFace> FaceList { get; set; }
		public string Material { get; set; }
		public int Index { get; set; }
		public bool WorldCoords { get; set; }
		public bool Visible { get; set; }
	}

	public class ImportedVertex
	{
		public Vector3 Position { get; set; }
		public float[] Weights { get; set; }
		public byte[] BoneIndices { get; set; }
		public Vector3 Normal { get; set; }
		public float[] UV { get; set; }
		public Vector4 Tangent { get; set; }
	}

	public class ImportedVertexWithColour : ImportedVertex
	{
		public Color4 Colour { get; set; }
	}

	public class ImportedFace
	{
		public int[] VertexIndices { get; set; }
	}

	public class ImportedBone
	{
		public string Name { get; set; }
		public Matrix Matrix { get; set; }
	}

	public class ImportedMaterial
	{
		public string Name { get; set; }
		public Color4 Diffuse { get; set; }
		public Color4 Ambient { get; set; }
		public Color4 Specular { get; set; }
		public Color4 Emissive { get; set; }
		public float Power { get; set; }
		public string[] Textures { get; set; }
		public Vector2[] TexOffsets { get; set; }
		public Vector2[] TexScales { get; set; }
	}

	public class ImportedTexture
	{
		public string Name { get; set; }
		public byte[] Data { get; set; }
		public bool isCompressed { get; protected set; }

		public ImportedTexture()
		{
		}

		public ImportedTexture(string path) : this(File.OpenRead(path), Path.GetFileName(path)) { }

		public ImportedTexture(Stream stream, string name)
		{
			Name = name;
			using (BinaryReader reader = new BinaryReader(stream))
			{
				Data = reader.ReadToEnd();
			}

			if (Path.GetExtension(name).ToUpper() == ".TGA")
			{
				isCompressed = Data[2] == 0x0A;
			}
		}
	}

	public interface ImportedAnimation
	{
		// List<dynamic : ImportedAnimationTrack> TrackList { get; set; }
	}

	public abstract class ImportedAnimationTrackContainer<TrackType> : ImportedAnimation where TrackType : ImportedAnimationTrack
	{
		public List<TrackType> TrackList { get; set; }

		public TrackType FindTrack(string name)
		{
			return TrackList.Find
			(
				delegate(TrackType track)
				{
					return track.Name == name;
				}
			);
		}
	}

	public class ImportedKeyframedAnimation : ImportedAnimationTrackContainer<ImportedAnimationKeyframedTrack> { }

	public class ImportedSampledAnimation : ImportedAnimationTrackContainer<ImportedAnimationSampledTrack>
	{
		public string Name { get; set; }
	}

	public abstract class ImportedAnimationTrack
	{
		public string Name { get; set; }
	}

	public class ImportedAnimationKeyframedTrack : ImportedAnimationTrack
	{
		public ImportedAnimationKeyframe[] Keyframes { get; set; }
	}

	public class ImportedAnimationKeyframe
	{
		public Vector3 Scaling { get; set; }
		public Quaternion Rotation { get; set; }
		public Vector3 Translation { get; set; }
	}

	public class ImportedAnimationSampledTrack : ImportedAnimationTrack
	{
		public struct ABC
		{
			public float A, B, C;
		}

		public Vector3?[] Scalings;
		public ABC?[] HCScaleX, HCScaleY, HCScaleZ;
		public Quaternion?[] Rotations;
		public ABC?[] HCRotateX, HCRotateY, HCRotateZ, HCRotateW;
		public Vector3?[] Translations;
		public ABC?[] HCTranslateX, HCTranslateY, HCTranslateZ;
		public float?[] Curve;
		public ABC?[] HCCurve;
	}

	public class ImportedMorph
	{
		/// <summary>
		/// Target mesh name
		/// </summary>
		public string Name { get; set; }
		public string ClipName { get; set; }
		public List<Tuple<float, int, int>> Channels { get; set; }
		public List<ImportedMorphKeyframe> KeyframeList { get; set; }
		public List<ushort> MorphedVertexIndices { get; set; }
	}

	public class ImportedMorphKeyframe
	{
		/// <summary>
		/// Blend shape name
		/// </summary>
		public string Name { get; set; }
		public List<ImportedVertex> VertexList { get; set; }
		public List<ushort> MorphedVertexIndices { get; set; }
		public float Weight { get; set; }
	}

	public static class ImportedHelpers
	{
		public static ImportedFrame FindFrame(String name, ImportedFrame root)
		{
			String[] path = name.Split('/');
			ImportedFrame frame = root;
			bool found = false;
			for (int i = 0; i < path.Length; i++)
			{
				found = false;
				for (int j = 0; j < frame.Count; j++)
				{
					if (frame[j].Name == path[i])
					{
						frame = frame[j];
						found = true;
						break;
					}
				}
				if (!found)
				{
					Report.ReportLog("Wrong path= " + name + ". Child " + path[i] + " not found.");
					return null;
				}
			}
			return found ? frame : root.Name == name ? root : null;
		}

		public static Matrix WorldTransform(ImportedFrame frame)
		{
			Matrix world = frame.Matrix;
			while (frame.Parent != null)
			{
				frame = frame.Parent;
				world = world * frame.Matrix;
			}
			return world;
		}

		public static ImportedMesh FindMesh(String frameName, List<ImportedMesh> importedMeshList)
		{
			foreach (ImportedMesh mesh in importedMeshList)
			{
				if (mesh.Name == frameName)
				{
					return mesh;
				}
			}

			return null;
		}

		public static ImportedMesh FindMesh(ImportedFrame frame, List<ImportedMesh> importedMeshList)
		{
			StringBuilder sb = new StringBuilder();
			string framePath;
			ImportedFrame root = frame;
			while (root.Parent != null)
			{
				sb.Insert(0, root.Name).Insert(0, '/');
				root = root.Parent;
			}
			if (sb.Length > 0)
			{
				sb.Remove(0, 1);
				framePath = sb.ToString();
			}
			else
			{
				framePath = frame.Name;
			}

			foreach (ImportedMesh mesh in importedMeshList)
			{
				if (mesh.Name == framePath)
				{
					return mesh;
				}
			}

			return null;
		}

		public static ImportedMaterial FindMaterial(String name, List<ImportedMaterial> importedMats)
		{
			foreach (ImportedMaterial mat in importedMats)
			{
				if (mat.Name == name)
				{
					return mat;
				}
			}

			return null;
		}

		public static ImportedTexture FindTexture(string name, List<ImportedTexture> importedTextureList)
		{
			if (name == null || name == string.Empty)
			{
				return null;
			}

			foreach (ImportedTexture tex in importedTextureList)
			{
				if (tex.Name == name)
				{
					return tex;
				}
			}

			return null;
		}

		public static ImportedAnimation FindAnimation(string name, List<ImportedAnimation> importedAnimations)
		{
			if (name == null || name == string.Empty)
			{
				return null;
			}

			foreach (ImportedAnimation anim in importedAnimations)
			{
				if (anim is ImportedSampledAnimation && ((ImportedSampledAnimation)anim).Name == name)
				{
					return anim;
				}
			}

			return null;
		}
	}
}
