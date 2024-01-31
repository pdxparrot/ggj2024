extends Skeleton3D

func enable_partial_ragdoll(LeftUpLeg):
    var physical_bone = $Skeleton3D.get_node(LeftUpLeg)
    if physical_bone and physical_bone is PhysicalBone3D:
        physical_bone.simulate_physics = true
