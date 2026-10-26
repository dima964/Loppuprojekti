================================================================
  PSX SOVIET BUNKER PACK
  by juggypuggy
================================================================

63 game-ready low poly models for building Soviet bunker
interiors. Modular walls, floor and ceiling plus props, weapons,
furniture and clutter.

Everything shares ONE 1024x1024 texture and ONE material, so the
whole pack batches as a single draw call.

Total: 3856 triangles across 63 meshes.


----------------------------------------------------------------
  WHAT'S IN THE BOX
----------------------------------------------------------------

  GLB/        63 .glb files. Texture embedded, nearest-neighbour
              filtering already baked in. Drag straight into
              Blender, Unreal, Unity or Godot.

  FBX/        63 .fbx files (binary, FBX 7400). Texture embedded
              and connected to the material. The atlas PNG is
              also in this folder in case your importer wants it
              as a separate file.

  Textures/   The shared atlas, T_PSXPack_01_D.png.


----------------------------------------------------------------
  IMPORT NOTES  -  READ THIS ONE
----------------------------------------------------------------

If the texture looks BLURRY on import, that is a filter setting,
not a broken file. The whole look depends on nearest-neighbour
sampling.

  Blender   Shading tab, select the Image Texture node,
            set Interpolation to "Closest".

  Unreal    Double-click the texture, set Filter to "Nearest".
            Turn off sRGB mip blur if you see softening.

  Unity     Select the texture, Filter Mode = Point (no filter),
            Compression = None.

  Godot     Texture import settings, turn Filter OFF.

Scale: models are authored in metres. Unreal users import at
100 units per metre (the FBX files are already in centimetres).

Orientation: Z up, front faces -Y, pivot at floor level and
centred on X/Y, so everything drops straight onto a surface.


----------------------------------------------------------------
  CONTENTS
----------------------------------------------------------------

ENVIRONMENT (16)
  SM_Env_CableRun_01                    84 tris
  SM_Env_CeilingConcrete_01             32 tris
  SM_Env_DoorFrame_01                   34 tris
  SM_Env_DoorMetal_01                   22 tris
  SM_Env_FloorConcrete_01               32 tris
  SM_Env_FloorDrain_01                  10 tris
  SM_Env_FuseBox_01                     12 tris
  SM_Env_LadderMetal_01                132 tris
  SM_Env_MapBoard_01                     2 tris
  SM_Env_PipeRunH_01                   112 tris
  SM_Env_PipeRun_01                     84 tris
  SM_Env_PipeValve_01                   56 tris
  SM_Env_Poster_01                       2 tris
  SM_Env_WallClock_01                    8 tris
  SM_Env_WallConcrete_01                48 tris
  SM_Env_WallVent_01                    12 tris

PROPS (47)
  SM_Prop_AmmoCan_01                    24 tris
  SM_Prop_Armchair_01                   94 tris
  SM_Prop_BarrelBlue_01                 96 tris
  SM_Prop_BarrelRust_01                 96 tris
  SM_Prop_BarrelSteel_01                96 tris
  SM_Prop_Bookshelf_01                 102 tris
  SM_Prop_BottleVodka_01               120 tris
  SM_Prop_BucketMetal_01               152 tris
  SM_Prop_CanFood_01                    96 tris
  SM_Prop_CanFood_02                    96 tris
  SM_Prop_CanFood_03                    96 tris
  SM_Prop_CartonBox_01                  14 tris
  SM_Prop_CartridgeBox_01               14 tris
  SM_Prop_Cartridge_01                  40 tris
  SM_Prop_Chair_01                      70 tris
  SM_Prop_Cot_01                        60 tris
  SM_Prop_CrateAmmo_01                  36 tris
  SM_Prop_CrateWood_01                  60 tris
  SM_Prop_Dustbin_01                    96 tris
  SM_Prop_FileCabinet_01                24 tris
  SM_Prop_FireExtinguisher_01          112 tris
  SM_Prop_FirstAidBox_01                12 tris
  SM_Prop_Footlocker_01                 36 tris
  SM_Prop_FuelCanRed_01                 48 tris
  SM_Prop_GasCylinder_01               108 tris
  SM_Prop_GasMask_01                    28 tris
  SM_Prop_Helmet_01                     72 tris
  SM_Prop_Kettle_01                    108 tris
  SM_Prop_LampHanging_01                84 tris
  SM_Prop_LockerMetal_01               194 tris
  SM_Prop_Mug_01                        96 tris
  SM_Prop_PalletWood_01                 96 tris
  SM_Prop_Papers_01                     10 tris
  SM_Prop_Pistol_01                     12 tris
  SM_Prop_PreserveJar_01                80 tris
  SM_Prop_Radio_01                      24 tris
  SM_Prop_RifleBolt_01                  12 tris
  SM_Prop_RopeCoil_01                   12 tris
  SM_Prop_SMG_01                        12 tris
  SM_Prop_SandbagStack_01               12 tris
  SM_Prop_ShelfMetal_01                 96 tris
  SM_Prop_Sofa_01                      142 tris
  SM_Prop_StoolWood_01                  84 tris
  SM_Prop_TableWood_01                  72 tris
  SM_Prop_Toolbox_01                    24 tris
  SM_Prop_TrainingDummy_01              36 tris
  SM_Prop_WeaponRack_01                 70 tris


----------------------------------------------------------------
  LICENSE
----------------------------------------------------------------

CC0 1.0 - public domain. Do whatever you want with it.
Commercial use, modification, redistribution, reselling, all
fine. No credit required.

See LICENSE.txt.


----------------------------------------------------------------
  MORE
----------------------------------------------------------------

More packs at  https://juggypuggy.itch.io

Found a problem, or want something added to the pack? Tell me on
the itch page and I will look at it.

Thanks for downloading. Go make something weird.
