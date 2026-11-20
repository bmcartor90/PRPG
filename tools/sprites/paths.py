# Where everything lives. Every script imports this, so they run from any working directory.
import os
TOOLS=os.path.dirname(os.path.abspath(__file__))
ROOT=os.path.normpath(os.path.join(TOOLS,'..','..'))
WORK=os.path.join(TOOLS,'work')          # working files: frame boxes, metadata, fist labels, contact sheets
LAB=os.path.join(ROOT,'feel-lab')     # the lab and the images it loads
ASSETS=os.path.join(ROOT,'assets')
# Sheet ids (the upload ids the scripts were written against) -> source file in assets/. Add new sheets here.
SHEETS={
 '67ddc9c4':'sprites/lucca/source/Meshy_AI_Pixel_Character_Sprite_Sheet.png',
 '9aa0aed0':'sprites/lucca/source/Meshy_AI_Character_Action_Sprite_Sheet.png',
 'ebd959ba':'sprites/lucca/source/Meshy_AI_Lucca_Ledge_and_Lunge_Sheet.png',
 '31b2f36f':'sprites/lucca/source/Meshy_AI_Lucca_Rope_and_Climb_Sheet.png',
 '25cd7dc8':'sprites/lucca/source/Meshy_AI_Lucca_Weapon_Draw_Sheet.png',
 '51f22a66':'sprites/lucca/source/Meshy_AI_Lucca_Invisible_Weapons_Sheet.png',
 '442ad3cf':'sprites/lucca/source/Meshy_AI_Lucca_Moving_Draws_Sheet.png',
 '1dce5bb6':'sprites/lucca/source/Meshy_AI_Lucca_Draw_Opposite_Hand_Sheet.png',
 'f244808c':'sprites/lucca/source/Meshy_AI_Lucca_Moving_Draws_Inverted_Sheet.png',
 'b4e1e56c':'sprites/items/source/Meshy_AI_Lucca_Weapons_Equipment_Sheet.png',
 '65975ccc':'sprites/nina/source/Meshy_AI_Nina_Walk_and_Run_Sheet.png',
 '72dd2f7a':'sprites/nina/source/Meshy_AI_Nina_Invisible_Combat_Sheet.png',
 '0c517fb8':'sprites/nina/source/Meshy_AI_Nina_Moving_Draws_Sheet.png',
 'c67b52a8':'sprites/nina/source/Meshy_AI_Nina_Moving_Draws_Inverted_Sheet.png',
 '65ec7e6d':'sprites/nina/source/Meshy_AI_Nina_Rope_and_Wall_Sheet.png',
 'b04cdec0':'sprites/nina/source/Meshy_AI_Nina_Ledge_and_Lunge_Sheet.png',
 '4d7d789d':'sprites/nina/source/Meshy_AI_Nina_Action_Acrobatics_Sheet.png',
 '7997c641':'sprites/ramorand/source/Meshy_AI_ramorand_walk_and_run_sheet.png',
 '9bc3dcb6':'sprites/ramorand/source/Meshy_AI_ramorand_action_acrobatics_sheet_1.png',
 'f8aa2c67':'sprites/ramorand/source/Meshy_AI_ramorand_ledge_bare_hands_sheet.png',
 '229f252c':'sprites/ramorand/source/Meshy_AI_ramorand_ledge_and_lunge_sheet.png',
 'b64f54a6':'sprites/ramorand/source/Meshy_AI_ramorand_rope_and_climb_sheet.png',
 '5e5e0121':'sprites/ramorand/source/Meshy_AI_ramorand_invisible_combat_sheet.png',
 '61b27d1a':'sprites/items/source/Meshy_AI_ramorand_weapons_equipment_sheet.png',
 '6dd62cb4':'tiles/crystal/source/Meshy_AI_crystal_platform_tileset.png',
 '474ce6bb':'tiles/scaffold/source/Meshy_AI_miner_scaffold_tileset.png',
 'ab22f874':'tiles/crystal/source/Meshy_AI_crystal_cavern_solid_wall_tileset.png',
 '0c686896':'props/doors/source/Meshy_AI_environment_doors_and_openings_sheet.png',
 '17425c7e':'props/doors/source/Meshy_AI_locked_doors_and_dormant_portal_sheet.png',
 '6d34fc5c':'props/doors/source/Meshy_AI_shimmering_portal_loop_sheet.png',
}
def SRC(i):
    """Path of a source sheet, by id or by a path under assets/."""
    return os.path.join(ASSETS,SHEETS.get(i,i))
os.makedirs(WORK,exist_ok=True)
