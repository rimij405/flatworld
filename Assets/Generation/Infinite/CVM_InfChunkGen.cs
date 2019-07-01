﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace CombinedVoxelMesh {
	public struct CVM_Chunk {
		public Vector2Int pos;
		public readonly CombinedVoxelMesh CVM;

		public CVM_Chunk(Vector2Int pos, CombinedVoxelMesh cVM) {
			this.pos = pos;
			CVM = cVM;
		}
	}

	/// <summary> Generate Infinite Chunks </summary>
	public class CVM_InfChunkGen : MonoBehaviour {
		/// <summary> Size of world in chunks </summary>
		public Vector2Int size = Vector2Int.one;
		public GameObject chunkPrefab;
		public GameObject cam;

		//[HideInInspector]
		//public CombinedVoxelMesh[] chunks;
		public CVM_Chunk[] chunks;
		Dictionary<Vector2Int, CombinedVoxelMesh> chunkMap;
		Dictionary<Vector2Int, Voxel[]> voxelMap;

		int csx, csy, csz, r;

		Vector2Int viewPos = new Vector2Int(int.MinValue, int.MinValue);

		void Start() => StartCoroutine(GenChunks());

		IEnumerator GenChunks() {
			chunks = new CVM_Chunk[size.x * size.y];
			chunkMap = new Dictionary<Vector2Int, CombinedVoxelMesh>(chunks.Length);
			voxelMap = new Dictionary<Vector2Int, Voxel[]>(chunks.Length);

			Vector3Int chunkSize = chunkPrefab.GetComponent<CombinedVoxelMesh>().size;
			csx = chunkSize.x;
			csy = chunkSize.y;
			csz = chunkSize.z;

			r = size.x / 2;

			viewPos = GetViewpos();
			Vector2Int off = viewPos - new Vector2Int(size.x / 2, size.y / 2);

			for (int i = 0; i < chunks.Length; i++) {
				IndexToXZ(i, out int x, out int z);
				Vector2Int xz = new Vector2Int(x, z) + off;
				GameObject o = Instantiate(chunkPrefab, new Vector3(xz.x * chunkSize.x, 0f, xz.y * chunkSize.z), Quaternion.identity, transform);
				o.name = $"Chunk {i + 1}";
				print($"{o.name} generated");

				CombinedVoxelMesh CVM = o.GetComponent<CombinedVoxelMesh>();
				chunks[i] = new CVM_Chunk(xz, CVM);
				chunkMap[xz] = CVM;
				voxelMap[xz] = CVM.voxels;
				yield return null;
			}
		}

		void Update() {
			Vector2Int pos = GetViewpos();
			if (pos != viewPos) {
				//UpdatePositons(pos);
				StartCoroutine(UpdatePositons(pos));
			}
		}

		Vector2Int GetViewpos() => WorldToXZ(cam.transform.position);
		IEnumerator UpdatePositons(Vector2Int vpos, int msLimit = 100) {
			Stopwatch sw = new Stopwatch();
			sw.Start();
			Vector2Int p2 = viewPos + vpos;

			for (int i = 0; i < chunks.Length; i++) {
				CVM_Chunk c = chunks[i];
				Vector2Int dif = c.pos - vpos;
				if (Mathf.Max(Mathf.Abs(dif.x), Mathf.Abs(dif.y)) > r) {
					int x = p2.x - c.pos.x;
					int z = p2.y - c.pos.y;
					Vector2Int nPos = new Vector2Int(x, z);

					chunks[i].pos = nPos;
					CombinedVoxelMesh cvm = c.CVM;
					chunkMap[nPos] = cvm;

					//if (CombinedVoxelMesh.colliderHolder.activeSelf) CombinedVoxelMesh.colliderHolder.SetActive(false);
					GameObject o = cvm.gameObject;
					o.transform.position = new Vector3(x * csx, 0, z * csz);

					if (voxelMap.ContainsKey(nPos)) {
						cvm.voxels = voxelMap[nPos];
					}
					else {
						cvm.voxels = voxelMap[nPos] = new Voxel[cvm.voxels.Length];
						cvm.FillVoxels();
					}
					cvm.Regenerate();

					if (sw.ElapsedMilliseconds >= msLimit) {
						//if (!CombinedVoxelMesh.colliderHolder.activeSelf) CombinedVoxelMesh.colliderHolder?.SetActive(true);
						yield return null;
					}
				}
			}

			//if (!CombinedVoxelMesh.colliderHolder.activeSelf) CombinedVoxelMesh.colliderHolder?.SetActive(true);
			viewPos = vpos;
		}

		#region Conversions
		public void WorldToXZ(Vector3 world, out int x, out int z) {
			x = Mathf.FloorToInt(Mathf.Round(world.x) / csx);
			z = Mathf.FloorToInt(Mathf.Round(world.z) / csz);
		}
		public Vector2Int WorldToXZ(Vector3 world) {
			WorldToXZ(world, out int x, out int z);
			return new Vector2Int(x, z);
		}
		public int XZtoIndex(int x, int z) => x + z * size.x;
		public void IndexToXZ(int i, out int x, out int z) {
			x = i % size.x;
			z = (i / size.x) % size.y;
		}

		public CombinedVoxelMesh WorldToChunk(Vector3 world) {
			//int x, z;
			//WorldToXZ(world, out x, out z);
			//return XZtoChunk(x, z);
			return chunkMap[WorldToXZ(world)];
		}
		public CombinedVoxelMesh XZtoChunk(int x, int z) => chunks[XZtoIndex(x, z)].CVM;
		#endregion
	}
}