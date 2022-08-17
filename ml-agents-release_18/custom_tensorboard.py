import json
import pandas as pd
import tensorflow as tf
import os

run_number = 5
file_name = f"run_{run_number}.json"
f = open(os.path.join("custom_unity_log", file_name))
data = json.load(f)
df = pd.json_normalize(data["Items"])
food_polar_corr = df['globalPolarization'].corr(df['meanFoodIntensity'])
pred_polar_corr = df['globalPolarization'].corr(df['meanPredatorIntensity'])
speed_polar_corr = df['globalPolarization'].corr(df['meanSpeed'])
df['stockSim'] = df['globalPolarization'] + df['foodEaten']
print(f"group polarization / food intensity corr: {food_polar_corr}")
print(f"group polarization / predator intensity corr: {pred_polar_corr}")
print(f"group polarization / mean speed corr: {speed_polar_corr}")
subdir = f"run_{run_number}"
summary_dir1 = os.path.join("fish_tensor", subdir, "t1")
summary_writer1 = tf.summary.create_file_writer(summary_dir1)
i=0
for stat in data["Items"]:
    with summary_writer1.as_default():
        tf.summary.scalar(name="agent/food_eaten", data=stat["foodEaten"], step=stat["steps"])
        tf.summary.scalar(name="agent/total_agent_hit_count", data=stat["totalAgentHitCount"], step=stat["steps"])
        tf.summary.scalar(name="agent/total_wall_hit_count", data=stat["totalWallHitCount"], step=stat["steps"])
        tf.summary.scalar(name="agent/fish_eaten", data=stat["fishEaten"], step=stat["steps"])
        tf.summary.scalar(name="agent/avg_steps_near_wall", data=stat["avgFramesNearWall"], step=stat["steps"])
        tf.summary.scalar(name="agent/agent_satiated_ratio", data=stat["agentSatiatedRatio"], step=stat["steps"])
        tf.summary.scalar(name="agent/global_polarization", data=stat["globalPolarization"], step=stat["steps"])
        tf.summary.scalar(name="agent/mean_speed", data=stat["meanSpeed"], step=stat["steps"])
        tf.summary.scalar(name="agent/mean_food_intensity", data=stat["meanFoodIntensity"], step=stat["steps"])
        tf.summary.scalar(name="agent/mean_predator_intensity", data=stat["meanPredatorIntensity"], step=stat["steps"])
        tf.summary.scalar(name="agent/mean_direction", data=stat["meanDirection"], step=stat["steps"])
        tf.summary.scalar(name="corr/group_polarization vs food_intensity", data=food_polar_corr, step=stat["steps"])
        tf.summary.scalar(name="corr/group_polarization vs predator_intensity", data=pred_polar_corr, step=stat["steps"])
        tf.summary.scalar(name="corr/group_polarization vs mean_speed", data=speed_polar_corr, step=stat["steps"])
        tf.summary.scalar(name="modeling/stocksim", data=df['stockSim'].values[i], step=stat["steps"])
        i+=1

    summary_writer1.flush()
