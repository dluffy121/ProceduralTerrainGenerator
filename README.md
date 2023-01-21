# Procedural Terrain Generator

The idea of this project is to generate endless terrains procedurally by just modifying its properties like noise data, texture data, and much more to provide a more natural looking base mesh for a terrain. This type of procedural generation is helpful to provide a more random sense of the environment, and reduces artists efforts in creating different variation of a similar looking terrain. The procedural generation is possible using perlin noise and using its properties to give elevation to the terrain by controlling different properties like levels, its strength and attenuation factors, height curve, etc.

## Highlights
+ [Noise Map](#noise-map)
  + [Levels](#levels)
  + [Strength](#strength)
  + [Attenuation](#attenuation)
+ [Data](#data)
+ [Mesh Creation](#mesh-creation)
+ [Terrain Shader](#terrain-shader)
+ [Endless Terrain](#endless-terrain)
  + [Terrain Chunk](#terrain-chunk)
+ [References](#references)
+ [Dependencies](#dependencies)

## Noise Map
Perlin noise helps in achieving gradient type randomness in a texture. This texture is then used as a base to apply heights and colors to our mesh.

We use Unity's ***Mathf.PerlinNoise*** to sample from which helps in our noise map generation. But before sampling we use our 3 major properties of:

### Levels
It applies more detail to the noise map by combining different wave graphs. These levels are influenced by the diminishing strength and attenuation values.

![NoiseLevel](https://user-images.githubusercontent.com/43366313/213860920-6524f887-b9da-46af-b520-06fee6617410.gif)<br>*Noise Level*

### Strength
A factor that affects the amplitude of the waves of each level. It ranges between 1 to 0 and is halved for each level. This property gives a more diminishing detail for increasing levels.

$amplitude_{level} = strength^{-2({level}-1)}$

![NoiseStrength](https://user-images.githubusercontent.com/43366313/213861134-f3651783-21c6-41cf-9e73-70400b2803aa.gif)<br>*Noise Strength*

### Attenuation
A factor that affects the frequency of the waves of each level. It is halved for each level. This property helps increase the frequency with increasing levels giving more frequent detailing for finer waves.

$frequency_{level} = attenuation^{2({level}-1)}$

![NoiseAttenuation](https://user-images.githubusercontent.com/43366313/213861921-8d3e6f38-046f-4e18-818f-5b534acc1122.gif)<br>*Noise Attenuation*

## Data
Data is stored and used from the following class types:
1. [Noise Data](Assets/Scripts/Data/NoiseData.cs)
2. [Terrain Data](Assets/Scripts/Data/TerrainData.cs)
3. [Texture Data](Assets/Scripts/Data/TextureData.cs)

## Mesh Creation
Mesh is created from the noise map by calling ***MeshGenerator.GenerateMesh*** which takes in height curve, height multiplier, and its level of detail. Each of these meshes can be connected to each other, the connection between them takes care of the noise map offset and normals. The seamless feel is achieved by adding an extra line of vertices to the mesh's edge, which helps with calculating edge normals properly by considering the extra set of triangles.

## [Terrain Shader](Assets/Shaders/Terrain.shader)
The shader uses the various regions defined by the Texture Data asset and height data from the Terrain Data to shade the terrain with respective colors. The texture mapped onto the mesh by using a technique called triplanar mapping, which takes into consideration the normals and xyz projections at each pixel. This helps avoid stretching of the texture due to sudden changes in height.

## [Endless Terrain](Assets/Scripts/EndlessTerrain.cs)
The endless nature of terrain is achieved by generating meshes around the viewer and connecting them together. Depending upon the movement of the viewer the meshes are generated on the fly. Major optimization is done by adopting Level of Detail technique. 

### [Terrain Chunk](Assets/Scripts/TerrainChunk.cs)
Each mesh is treated as a terrain chunk instance which has the renderer, and collider component attached to it. It also holds various LOD meshes. These meshes are then assigned to its renderer and collider.

![EndlessTerrain](https://user-images.githubusercontent.com/43366313/213862790-0af9eef5-a791-4152-94c5-8937a9b6cfa8.gif)<br>*Endless Terrain*

## References
1. Sebastian Lague's [Procedural Terrain Generation](https://youtube.com/playlist?list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3)

## Dependencies
1. [Unity's Starter Assets - Third Person Character Controller](https://assetstore.unity.com/packages/essentials/starter-assets-third-person-character-controller-196526)
