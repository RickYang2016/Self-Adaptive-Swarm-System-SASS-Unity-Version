# -*- coding: utf-8 -*-
from __future__ import division
import gambit
import fractions
import math
import argparse
import numpy as np
from sympy import *
import scipy.stats as st
import json

# Author: Qin Yang

# This version does not add the instinct reaction to avoid sacrifice 
# This version also does not add the Mini−Modifying Sub−Tree(MMST) making the decision process in the dynamical environment efficiently.
# explorers' average attacking ability: 1; explorers' average defending ability: 2.
# monsters' average attacking ability: 3; monsters' average defending ability: 4.

class Decision_making():

	explorers_list = []		# [{'unit_attacking_energy_cost': 10, 'energy_level': 97.42, 'health_power': 93.17, 'agent_area': 3, 'agent_velocity': 3, 'adversary_distance': 30}, \
							#  {'unit_attacking_energy_cost': 15, 'energy_level': 97.42, 'health_power': 93.17, 'agent_area': 3, 'agent_velocity': 3, 'adversary_distance': 10}, ...]
	
	monsters_list = []		# the same as explorers_list

	# explorer and monster level-1 tactics
	explorers_level_1_strategy = {'attack': 0, 'defend': 0, 'patrol': 0}
	monsters_level_1_strategy = {'attack': 0, 'defend': 0, 'patrol': 0}

	# explorer and monster level-2 tactics
	explorers_level_2_strategy = {'nearest': 0, 'lowest_attacking_ability': 0, 'highest_attacking_ability': 0}
	monsters_level_2_strategy = {'nearest': 0, 'lowest_attacking_ability': 0, 'highest_attacking_ability': 0}

	# explorer and monster level-3 tactics
	explorers_level_3_strategy = {'one_group': 0, 'two_groups': 0, 'three_groups': 0}
	monsters_level_3_strategy = {'independent': 0, 'dependent': 0}

	def __init__(self, \
				 explorers_list, \
				 monsters_list):

		self.explorers_list = explorers_list
		self.monsters_list = monsters_list

	def explorer_attacking_ability(self, energy_level):

		a = 0.0111

		te = a * energy_level

		return te

	def explorer_defending_ability(self, energy_level):

		a = 0.0222

		re = a * energy_level

		return re

	def monster_attacking_ability(self, energy_level):

		a = 0.0107

		tm = a * energy_level

		return tm

	def monster_defending_ability(self, energy_level):

		a = 0.0143

		rm = a * energy_level

		return rm

	def distance_energy(self, d, num_explorers, num_monsters):

		b11 = 3
		result = (num_explorers - num_monsters) * b11 * d

		return result

		# print("result1: ", result)

	def communication_energy(self, explorer_average_AD, agent_velocity, num_explorers):

		b13 = 5
		result = 0 

		for i in range(99):
			result = result + b13 * num_explorers * i * st.poisson(explorer_average_AD/agent_velocity).pmf(i)

		# print("result3: ", result)

		return result

	def attacking_energy(self, num_explorers, num_monsters, p, q, explorer_area, monster_area):

		b12e = 1
		b12m = 3
		result1 = 0
		result2 = 0

		for i in range(99):
			result1 = result1 + b12e * i * p * num_explorers * num_monsters * st.poisson(monster_area).pmf(i)

		for j in range(99):
			result2 = result2 + b12m * j * q * num_monsters * num_explorers * st.poisson(explorer_area).pmf(j)
		
		result = result1 - result2

		# print("result2: ", result)

		return result

	def expected_enery_utility(self, d, agent_velocity, num_explorers, num_monsters, p, q, explorer_area, monster_area):

		ed = self.distance_energy(d, num_explorers, num_monsters)
		et = self.attacking_energy(num_explorers, num_monsters, p, q, explorer_area, monster_area)
		ec = self.communication_energy(d, agent_velocity, num_explorers)

		result = ed + et + ec

		# print("expected_enery_utility is: ", result)

		return result

	def expected_HP_utility(self, k, t, r, h, s, g, explorer_area, monster_area):

		ce = 3
		cm = 1
		result1 = 0
		result2 = 0

		for i in range(99):
			result1 = result1 + st.poisson(monster_area).pmf(i) * ce * (t + h) * i

		for j in range(99):
			result2 = result2 + st.poisson(explorer_area).pmf(j) * cm * (r + s) * j

		result = k * result1 - g * result2

		print("expected_HP_utility is: ", result)

		return result

	def winning_probability(self, t, r, h, s, num_explorers, num_monsters):

		a1 = 0.3

		if 0 < num_monsters/num_explorers <= 1/5:		# attack_e vs defend_m
			a2 = 1.3
			a3 = 0.5
			a4 = 0.8
			a5 = 1.3	
		elif 1/5 < num_monsters/num_explorers <= 1/3:	# attack_e vs attack_m
			a2 = 0.3
			a3 = 0.5
			a4 = 1
			a5 = 0.8
		elif 1/3 < num_monsters/num_explorers <= 4/9:	# defend_e vs defend_m
			a2 = 1.1
			a3 = 0.3
			a4 = 1.3
			a5 = 0.8	
		elif 4/9 < num_monsters/num_explorers:			# defend_e vs attack_m
			a2 = 0.9
			a3 = 0.7
			a4 = 1.1
			a5 = 0.6
		else:
			print("Something was worng about the winning_probability coefficient!")

		result = math.pow(a1 * (a2 * t + a3 * h)/(a4 * r + a5 * s), num_monsters/num_explorers)

		# print("winning_probability is: ", result)

		return result

	def lowest_level_utility(self, the_explorer_attacking_level, the_monster_attacking_level, the_explorer_defending_level, the_monster_defending_level, num_explorers, num_monsters, lee, monster_area):

		explorer_area = num_explorers * lee

		if 0 < num_monsters/num_explorers <= 1/5:		# one group
			a = 0.3
			b = 0.5
			c = 1
		elif 1/5 < num_monsters/num_explorers <= 1/3:	# two groups
			a = 1.3
			b = 0.5
			c = 0.8
		elif 1/3 < num_monsters/num_explorers:	# three groups
			a = 1
			b = 1
			c = 1
		else:
			print("Something was worng about the winning_probability coefficient!")

		m = np.array([[int(self.expected_HP_utility(1, 4 * the_explorer_attacking_level/3, 4 * the_monster_attacking_level/3, 2 * the_explorer_defending_level/3, 2 * the_monster_defending_level/3, num_monsters, a * explorer_area, monster_area)), \
					   int(self.expected_HP_utility(1, 4 * the_explorer_attacking_level/3, 4 * the_monster_attacking_level/3, 2 * the_explorer_defending_level/3, 2 * the_monster_defending_level/3, 1, a * explorer_area, monster_area))], \
					  [int(self.expected_HP_utility(2, 4 * the_explorer_attacking_level/3, 4 * the_monster_attacking_level/3, 2 * the_explorer_defending_level/3, 2 * the_monster_defending_level/3, num_monsters, b * explorer_area, monster_area)), \
					   int(self.expected_HP_utility(2, 4 * the_explorer_attacking_level/3, 4 * the_monster_attacking_level/3, 2 * the_explorer_defending_level/3, 2 * the_monster_defending_level/3, 1, b * explorer_area, monster_area))], \
					  [int(self.expected_HP_utility(3, 4 * the_explorer_attacking_level/3, 4 * the_monster_attacking_level/3, 2 * the_explorer_defending_level/3, 2 * the_monster_defending_level/3, num_monsters, c * explorer_area, monster_area)), \
					   int(self.expected_HP_utility(3, 4 * the_explorer_attacking_level/3, 4 * the_monster_attacking_level/3, 2 * the_explorer_defending_level/3, 2 * the_monster_defending_level/3, 1, c * explorer_area, monster_area))]], dtype = gambit.Rational)

		g = gambit.Game.from_arrays(m, -m)

		z = gambit.nash.enummixed_solve(g)

		return z

	def nash_equilibrium(self):

		if len(self.monsters_list) == 0:
			self.explorers_level_1_strategy['patrol'] = 1.0
			self.monsters_level_1_strategy['patrol'] = 1.0
			
		elif len(self.monsters_list) != 0:
			tmpv_e = []
			tmpa_e = []
			tmpd_e = []
			tmpi_e = []
			tmpr_e = []
			tmpa_m = []
			tmpd_m = []
			# tmph_m = []
			# tmpn_m = []
			tmpl_m = []
			tmpr_m = []

			num_explorers = len(self.explorers_list)
			num_monsters = len(self.monsters_list)

			for i in range(num_explorers):
				tmpr_e.append(self.explorers_list[i]['agent_area'])

			explorer_area = np.mean(tmpr_e)

			for i in range(num_monsters):
				tmpr_m.append(self.monsters_list[i]['agent_area'])

			monster_area = np.mean(tmpr_m)

			for i in range(num_explorers):
				tmpi_e.append(self.explorers_list[i]['adversary_distance'])

			explorer_average_AD = np.mean(tmpi_e)

			for i in range(num_explorers):
				tmpv_e.append(self.explorers_list[i]['agent_velocity'])

			agent_velocity = np.mean(tmpv_e)

			for i in range(num_explorers):
				tmpa_e.append(self.explorer_attacking_ability(self.explorers_list[i]['energy_level']))

			explorer_attacking_level = np.mean(tmpa_e)

			for i in range(num_explorers):
				tmpd_e.append(self.explorer_defending_ability(self.explorers_list[i]['energy_level']))

			explorer_defending_level = np.mean(tmpd_e)

			for i in range(num_monsters):
				tmpa_m.append(self.monster_attacking_ability(self.monsters_list[i]['energy_level']))

			monster_attacking_level = np.mean(tmpa_m)

			for i in range(num_monsters):
				tmpd_m.append(self.monster_defending_ability(self.monsters_list[i]['energy_level']))

			monster_defending_level = np.mean(tmpd_m)

			print("num_explorers: ", num_explorers)
			print("num_monsters: ", num_monsters)
			print("explorer_average_AD: ", explorer_average_AD)
			print("agent_velocity: ", agent_velocity)
			print("explorer_attacking_level: ", explorer_attacking_level)
			print("explorer_defending_level: ", explorer_defending_level)
			print("monster_attacking_level: ", monster_attacking_level)
			print("monster_defending_level: ", monster_defending_level)

			m0 = np.array([[int(self.winning_probability(4 * explorer_attacking_level/3, 4 * monster_attacking_level/3, 2 * explorer_defending_level/3, 2 * monster_defending_level/3, num_explorers, num_monsters) * 1000), \
							int(self.winning_probability(2 * explorer_attacking_level/3, 4 * monster_attacking_level/3, 4 * explorer_defending_level/3, 2 * monster_defending_level/3, num_explorers, num_monsters) * 1000)], \
						   [int(self.winning_probability(4 * explorer_attacking_level/3, 2 * monster_attacking_level/3, 2 * explorer_defending_level/3, 4 * monster_defending_level/3, num_explorers, num_monsters) * 1000), \
						    int(self.winning_probability(2 * explorer_attacking_level/3, 2 * monster_attacking_level/3, 4 * explorer_defending_level/3, 4 * monster_defending_level/3, num_explorers, num_monsters) * 1000)]], dtype = gambit.Rational)

			g0 = gambit.Game.from_arrays(m0, 1000 - m0)

			z = gambit.nash.enummixed_solve(g0)

			for i in range(len(z[0])):
				self.explorers_level_1_strategy['attack'] = z[0][0] + 0.0
				self.explorers_level_1_strategy['defend'] = z[0][1] + 0.0
				self.monsters_level_1_strategy['attack'] = z[0][2] + 0.0
				self.monsters_level_1_strategy['defend'] = z[0][3] + 0.0

			if self.explorers_level_1_strategy['attack'] == 1 and self.monsters_level_1_strategy['attack'] == 1:
				tmpu_e = []

				for i in range(len(self.explorers_list)):
					tmpu_e.append(self.explorers_list[i]['unit_attacking_energy_cost'])

				explorer_average_UAEC = np.mean(tmpu_e)

				tmpn_m = self.monsters_list
				tmpl_m = self.monsters_list

				tmpn_m.sort(key = lambda kk:(kk.get('adversary_distance', 0)))
				tmpl_m.sort(key = lambda kk:(kk.get('energy_level', 0)))

				nearest_monster = tmpn_m[0]['unit_attacking_energy_cost']
				lowest_attacking_ability_monster = tmpl_m[0]['unit_attacking_energy_cost']

				m1 = np.array([[int(self.expected_enery_utility(explorer_average_AD/2, agent_velocity, num_explorers, num_monsters, explorer_average_UAEC, nearest_monster, explorer_area, monster_area)), \
								int(self.expected_enery_utility(explorer_average_AD/2, agent_velocity, num_explorers, num_monsters, 5 * explorer_average_UAEC/6, nearest_monster, explorer_area, monster_area))], \
							   [int(self.expected_enery_utility(explorer_average_AD/2, agent_velocity, num_explorers, num_monsters, explorer_average_UAEC, lowest_attacking_ability_monster, explorer_area, monster_area)), \
							    int(self.expected_enery_utility(explorer_average_AD/2, agent_velocity, num_explorers, num_monsters, 5 * explorer_average_UAEC/6, lowest_attacking_ability_monster, explorer_area, monster_area))]], dtype = gambit.Rational)

				g1 = gambit.Game.from_arrays(m1, -m1)

				z1 = gambit.nash.enummixed_solve(g1)

				for i in range(len(z1[0])):
					self.explorers_level_2_strategy['nearest'] = z1[0][0] + 0.0
					self.explorers_level_2_strategy['lowest_attacking_ability'] = z1[0][1] + 0.0
					self.monsters_level_2_strategy['nearest'] = z1[0][2] + 0.0
					self.monsters_level_2_strategy['lowest_attacking_ability'] = z1[0][3] + 0.0

				if self.explorers_level_2_strategy['nearest'] == 1 and self.monsters_level_2_strategy['nearest'] == 1:
					tmp_e = self.explorers_list
					tmp_m = self.monsters_list

					tmp_e.sort(key = lambda kk:(kk.get('adversary_distance', 0)))
					tmp_m.sort(key = lambda kk:(kk.get('adversary_distance', 0)))

					ee = tmp_e[0]['energy_level']

					the_explorer_attacking_level = self.explorer_attacking_ability(ee)
					the_explorer_defending_level = self.explorer_defending_ability(ee)

					em = tmp_m[0]['energy_level']

					the_monster_attacking_level = self.monster_attacking_ability(em)
					the_monster_defending_level = self.monster_defending_ability(em)

					gt11 = self.lowest_level_utility(the_explorer_attacking_level, the_monster_attacking_level, the_explorer_defending_level, the_monster_defending_level, num_explorers, num_monsters, explorer_area, monster_area)

					for i in range(len(gt11[0])):
						self.explorers_level_3_strategy['one_group'] = gt11[0][0] + 0.0
						self.explorers_level_3_strategy['two_groups'] = gt11[0][1] + 0.0
						self.explorers_level_3_strategy['three_groups'] = gt11[0][2] + 0.0
						self.monsters_level_3_strategy['independent'] = gt11[0][3] + 0.0
						self.monsters_level_3_strategy['dependent'] = gt11[0][4] + 0.0

				elif self.explorers_level_2_strategy['lowest_attacking_ability'] == 1 and self.monsters_level_2_strategy['nearest'] == 1:
					tmp_e = self.explorers_list
					tmp_m = self.monsters_list

					tmp_e.sort(key = lambda kk:(kk.get('adversary_distance', 0)))
					tmp_m.sort(key = lambda kk:(kk.get('energy_level', 0)))

					ee = tmp_e[0]['energy_level']

					the_explorer_attacking_level = self.explorer_attacking_ability(ee)
					the_explorer_defending_level = self.explorer_defending_ability(ee)

					em = tmp_m[0]['energy_level']

					the_monster_attacking_level = self.monster_attacking_ability(em)
					the_monster_defending_level = self.monster_defending_ability(em)

					gt12 = self.lowest_level_utility(the_explorer_attacking_level, the_monster_attacking_level, the_explorer_defending_level, the_monster_defending_level, num_explorers, num_monsters, explorer_area, monster_area)

					for i in range(len(gt12[0])):
						self.explorers_level_3_strategy['one_group'] = gt12[0][0] + 0.0
						self.explorers_level_3_strategy['two_groups'] = gt12[0][1] + 0.0
						self.explorers_level_3_strategy['three_groups'] = gt12[0][2] + 0.0
						self.monsters_level_3_strategy['independent'] = gt12[0][3] + 0.0
						self.monsters_level_3_strategy['dependent'] = gt12[0][4] + 0.0

				elif self.explorers_level_2_strategy['nearest'] == 1 and self.monsters_level_2_strategy['lowest_attacking_ability'] == 1:
					tmp_e = self.explorers_list
					tmp_m = self.monsters_list

					tmp_e.sort(key = lambda kk:(kk.get('energy_level', 0)))
					tmp_m.sort(key = lambda kk:(kk.get('adversary_distance', 0)))

					ee = tmp_e[0]['energy_level']

					the_explorer_attacking_level = self.explorer_attacking_ability(ee)
					the_explorer_defending_level = self.explorer_defending_ability(ee)

					em = tmp_m[0]['energy_level']

					the_monster_attacking_level = self.monster_attacking_ability(em)
					the_monster_defending_level = self.monster_defending_ability(em)

					gt13 = self.self.lowest_level_utility(the_explorer_attacking_level, the_monster_attacking_level, the_explorer_defending_level, the_monster_defending_level, num_explorers, num_monsters, explorer_area, monster_area)

					for i in range(len(gt13[0])):
						self.explorers_level_3_strategy['one_group'] = gt13[0][0] + 0.0
						self.explorers_level_3_strategy['two_groups'] = gt13[0][1] + 0.0
						self.explorers_level_3_strategy['three_groups'] = gt13[0][2] + 0.0
						self.monsters_level_3_strategy['independent'] = gt13[0][3] + 0.0
						self.monsters_level_3_strategy['dependent'] = gt13[0][4] + 0.0

				elif self.explorers_level_2_strategy['lowest_attacking_ability'] == 1 and self.monsters_level_2_strategy['lowest_attacking_ability'] == 1:
					tmp_e = self.explorers_list
					tmp_m = self.monsters_list

					tmp_e.sort(key = lambda kk:(kk.get('energy_level', 0)))
					tmp_m.sort(key = lambda kk:(kk.get('energy_level', 0)))

					ee = tmp_e[0]['energy_level']

					the_explorer_attacking_level = self.explorer_attacking_ability(ee)
					the_explorer_defending_level = self.explorer_defending_ability(ee)

					em = tmp_m[0]['energy_level']

					the_monster_attacking_level = self.monster_attacking_ability(em)
					the_monster_defending_level = self.monster_defending_ability(em)

					gt14 = self.lowest_level_utility(the_explorer_attacking_level, the_monster_attacking_level, the_explorer_defending_level, the_monster_defending_level, num_explorers, num_monsters, explorer_area, monster_area)

					for i in range(len(gt14[0])):
						self.explorers_level_3_strategy['one_group'] = gt14[0][0] + 0.0
						self.explorers_level_3_strategy['two_groups'] = gt14[0][1] + 0.0
						self.explorers_level_3_strategy['three_groups'] = gt14[0][2] + 0.0
						self.monsters_level_3_strategy['independent'] = gt14[0][3] + 0.0
						self.monsters_level_3_strategy['dependent'] = gt14[0][4] + 0.0

				else:
					print("E_attack-M_attack strategy has some problem!")

			elif self.explorers_level_1_strategy['attack'] == 1 and self.monsters_level_1_strategy['defend'] == 1:
				tmpu_e = []

				for i in range(len(self.explorers_list)):
					tmpu_e.append(self.explorers_list[i]['unit_attacking_energy_cost'])

				explorer_average_UAEC = np.mean(tmpu_e)

				tmpn_m = self.monsters_list
				tmph_m = self.monsters_list

				tmpn_m.sort(key = lambda kk:(kk.get('adversary_distance', 0)))
				tmph_m.sort(key = lambda kk:(kk.get('energy_level', 0)), reverse = True)

				nearest_monster = tmpn_m[0]['unit_attacking_energy_cost']
				highest_attacking_ability_monster = tmph_m[0]['unit_attacking_energy_cost']

				m2 = np.array([[int(self.expected_enery_utility(explorer_average_AD/2, agent_velocity, num_explorers, num_monsters, explorer_average_UAEC, nearest_monster, explorer_area, monster_area)), int(self.expected_enery_utility(explorer_average_AD/2, agent_velocity, num_explorers, num_monsters, 5 * explorer_average_UAEC/6, nearest_monster, explorer_area, monster_area))], \
							   [int(self.expected_enery_utility(explorer_average_AD/2, agent_velocity, num_explorers, num_monsters, explorer_average_UAEC, highest_attacking_ability_monster, explorer_area, monster_area)), int(self.expected_enery_utility(explorer_average_AD/2, agent_velocity, num_explorers, num_monsters, 5 * explorer_average_UAEC/6, highest_attacking_ability_monster, explorer_area, monster_area))]], dtype = gambit.Rational)

				g2 = gambit.Game.from_arrays(m2, -m2)

				z2 = gambit.nash.enummixed_solve(g2)

				for i in range(len(z2[0])):
					self.explorers_level_2_strategy['nearest'] = z2[0][0] + 0.0
					self.explorers_level_2_strategy['highest_attacking_ability'] = z2[0][1] + 0.0
					self.monsters_level_2_strategy['nearest'] = z2[0][2] + 0.0
					self.monsters_level_2_strategy['lowest_attacking_ability'] = z2[0][3] + 0.0

				if self.explorers_level_2_strategy['nearest'] == 1 and self.monsters_level_2_strategy['nearest'] == 1:
					tmp_e = self.explorers_list
					tmp_m = self.monsters_list

					tmp_e.sort(key = lambda kk:(kk.get('adversary_distance', 0)))
					tmp_m.sort(key = lambda kk:(kk.get('adversary_distance', 0)))

					ee = tmp_e[0]['energy_level']

					the_explorer_attacking_level = self.explorer_attacking_ability(ee)
					the_explorer_defending_level = self.explorer_defending_ability(ee)

					em = tmp_m[0]['energy_level']

					the_monster_attacking_level = self.monster_attacking_ability(em)
					the_monster_defending_level = self.monster_defending_ability(em)

					gt21 = self.lowest_level_utility(the_explorer_attacking_level, the_monster_attacking_level, the_explorer_defending_level, the_monster_defending_level, num_explorers, num_monsters, explorer_area, monster_area)

					for i in range(len(gt21[0])):
						self.explorers_level_3_strategy['one_group'] = gt21[0][0] + 0.0
						self.explorers_level_3_strategy['two_groups'] = gt21[0][1] + 0.0
						self.explorers_level_3_strategy['three_groups'] = gt21[0][2] + 0.0
						self.monsters_level_3_strategy['independent'] = gt21[0][3] + 0.0
						self.monsters_level_3_strategy['dependent'] = gt21[0][4] + 0.0

				elif self.explorers_level_2_strategy['highest_attacking_ability'] == 1 and self.monsters_level_2_strategy['nearest'] == 1:
					tmp_e = self.explorers_list
					tmp_m = self.monsters_list

					tmp_e.sort(key = lambda kk:(kk.get('adversary_distance', 0)))
					tmp_m.sort(key = lambda kk:(kk.get('energy_level', 0)), reverse = True)

					ee = tmp_e[0]['energy_level']

					the_explorer_attacking_level = self.explorer_attacking_ability(ee)
					the_explorer_defending_level = self.explorer_defending_ability(ee)

					em = tmp_m[0]['energy_level']

					the_monster_attacking_level = self.monster_attacking_ability(em)
					the_monster_defending_level = self.monster_defending_ability(em)

					gt22 = self.lowest_level_utility(the_explorer_attacking_level, the_monster_attacking_level, the_explorer_defending_level, the_monster_defending_level, num_explorers, num_monsters, explorer_area, monster_area)

					for i in range(len(gt22[0])):
						self.explorers_level_3_strategy['one_group'] = gt22[0][0] + 0.0
						self.explorers_level_3_strategy['two_groups'] = gt22[0][1] + 0.0
						self.explorers_level_3_strategy['three_groups'] = gt22[0][2] + 0.0
						self.monsters_level_3_strategy['independent'] = gt22[0][3] + 0.0
						self.monsters_level_3_strategy['dependent'] = gt22[0][4] + 0.0

				elif self.explorers_level_2_strategy['nearest'] == 1 and self.monsters_level_2_strategy['lowest_attacking_ability'] == 1:
					tmp_e = self.explorers_list
					tmp_m = self.monsters_list

					tmp_e.sort(key = lambda kk:(kk.get('energy_level', 0)))
					tmp_m.sort(key = lambda kk:(kk.get('adversary_distance', 0)))

					ee = tmp_e[0]['energy_level']

					the_explorer_attacking_level = self.explorer_attacking_ability(ee)
					the_explorer_defending_level = self.explorer_defending_ability(ee)

					em = tmp_m[0]['energy_level']

					the_monster_attacking_level = self.monster_attacking_ability(em)
					the_monster_defending_level = self.monster_defending_ability(em)

					gt23 = self.lowest_level_utility(the_explorer_attacking_level, the_monster_attacking_level, the_explorer_defending_level, the_monster_defending_level, num_explorers, num_monsters, explorer_area, monster_area)

					for i in range(len(gt23[0])):
						self.explorers_level_3_strategy['one_group'] = gt23[0][0] + 0.0
						self.explorers_level_3_strategy['two_groups'] = gt23[0][1] + 0.0
						self.explorers_level_3_strategy['three_groups'] = gt23[0][2] + 0.0
						self.monsters_level_3_strategy['independent'] = gt23[0][3] + 0.0
						self.monsters_level_3_strategy['dependent'] = gt23[0][4] + 0.0

				elif self.explorers_level_2_strategy['highest_attacking_ability'] == 1 and self.monsters_level_2_strategy['lowest_attacking_ability'] == 1:
					tmp_e = self.explorers_list
					tmp_m = self.monsters_list

					tmp_e.sort(key = lambda kk:(kk.get('energy_level', 0)))
					tmp_m.sort(key = lambda kk:(kk.get('energy_level', 0)), reverse = True)

					ee = tmp_e[0]['energy_level']

					the_explorer_attacking_level = self.explorer_attacking_ability(ee)
					the_explorer_defending_level = self.explorer_defending_ability(ee)

					em = tmp_m[0]['energy_level']

					the_monster_attacking_level = self.monster_attacking_ability(em)
					the_monster_defending_level = self.monster_defending_ability(em)

					gt24 = self.lowest_level_utility(the_explorer_attacking_level, the_monster_attacking_level, the_explorer_defending_level, the_monster_defending_level, num_explorers, num_monsters, explorer_area, monster_area)

					for i in range(len(gt24[0])):
						self.explorers_level_3_strategy['one_group'] = gt24[0][0] + 0.0
						self.explorers_level_3_strategy['two_groups'] = gt24[0][1] + 0.0
						self.explorers_level_3_strategy['three_groups'] = gt24[0][2] + 0.0
						self.monsters_level_3_strategy['independent'] = gt24[0][3] + 0.0
						self.monsters_level_3_strategy['dependent'] = gt24[0][4] + 0.0

				else:
					print("E_attack-M_attack strategy has some problem!")

			elif self.explorers_level_1_strategy['defend'] == 1 and self.monsters_level_1_strategy['attack'] == 1:
				tmpu_e = []

				for i in range(len(self.explorers_list)):
					tmpu_e.append(explorers_list[i]['unit_attacking_energy_cost'])

				explorer_average_UAEC = np.mean(tmpu_e)

				tmpn_m = self.monsters_list
				tmpl_m = self.monsters_list

				tmpn_m.sort(key = lambda kk:(kk.get('adversary_distance', 0)))
				tmpl_m.sort(key = lambda kk:(kk.get('energy_level', 0)))

				nearest_monster = tmpn_m[0]['unit_attacking_energy_cost']
				lowest_attacking_ability_monster = tmpl_m[0]['unit_attacking_energy_cost']

				m3 = np.array([[int(self.expected_enery_utility(explorer_average_AD/2, agent_velocity, num_explorers, num_monsters, explorer_average_UAEC, nearest_monster, explorer_area, monster_area)), \
								int(self.expected_enery_utility(explorer_average_AD/2, agent_velocity, num_explorers, num_monsters, 7 * explorer_average_UAEC/6, nearest_monster, explorer_area, monster_area))], \
							   [int(self.expected_enery_utility(explorer_average_AD/2, agent_velocity, num_explorers, num_monsters, explorer_average_UAEC, lowest_attacking_ability_monster, explorer_area, monster_area)), \
							    int(self.expected_enery_utility(explorer_average_AD/2, agent_velocity, num_explorers, num_monsters, 7 * explorer_average_UAEC/6, lowest_attacking_ability_monster, explorer_area, monster_area))]], dtype = gambit.Rational)

				g3 = gambit.Game.from_arrays(m3, -m3)

				z3 = gambit.nash.enummixed_solve(g3)

				for i in range(len(z3[0])):
					self.explorers_level_2_strategy['nearest'] = z3[0][0] + 0.0
					self.explorers_level_2_strategy['lowest_attacking_ability'] = z3[0][1] + 0.0
					self.monsters_level_2_strategy['nearest'] = z3[0][2] + 0.0
					self.monsters_level_2_strategy['highest_attacking_ability'] = z3[0][3] + 0.0

				if self.explorers_level_2_strategy['nearest'] == 1 and self.monsters_level_2_strategy['nearest'] == 1:
					tmp_e = self.explorers_list
					tmp_m = self.monsters_list

					tmp_e.sort(key = lambda kk:(kk.get('adversary_distance', 0)))
					tmp_m.sort(key = lambda kk:(kk.get('adversary_distance', 0)))

					ee = tmp_e[0]['energy_level']

					the_explorer_attacking_level = self.explorer_attacking_ability(ee)
					the_explorer_defending_level = self.explorer_defending_ability(ee)

					em = tmp_m[0]['energy_level']

					the_monster_attacking_level = self.monster_attacking_ability(em)
					the_monster_defending_level = self.monster_defending_ability(em)

					gt31 = self.lowest_level_utility(the_explorer_attacking_level, the_monster_attacking_level, the_explorer_defending_level, the_monster_defending_level, num_explorers, num_monsters, explorer_area, monster_area)

					for i in range(len(gt31[0])):
						self.explorers_level_3_strategy['one_group'] = gt31[0][0] + 0.0
						self.explorers_level_3_strategy['two_groups'] = gt31[0][1] + 0.0
						self.explorers_level_3_strategy['three_groups'] = gt31[0][2] + 0.0
						self.monsters_level_3_strategy['independent'] = gt31[0][3] + 0.0
						self.monsters_level_3_strategy['dependent'] = gt31[0][4] + 0.0

				elif self.explorers_level_2_strategy['lowest_attacking_ability'] == 1 and self.monsters_level_2_strategy['nearest'] == 1:
					tmp_e = self.explorers_list
					tmp_m = self.monsters_list

					tmp_e.sort(key = lambda kk:(kk.get('adversary_distance', 0)))
					tmp_m.sort(key = lambda kk:(kk.get('energy_level', 0)))

					ee = tmp_e[0]['energy_level']

					the_explorer_attacking_level = self.explorer_attacking_ability(ee)
					the_explorer_defending_level = self.explorer_defending_ability(ee)

					em = tmp_m[0]['energy_level']

					the_monster_attacking_level = self.monster_attacking_ability(em)
					the_monster_defending_level = self.monster_defending_ability(em)

					gt32 = self.lowest_level_utility(the_explorer_attacking_level, the_monster_attacking_level, the_explorer_defending_level, the_monster_defending_level, num_explorers, num_monsters, explorer_area, monster_area)

					for i in range(len(gt32[0])):
						self.explorers_level_3_strategy['one_group'] = gt32[0][0] + 0.0
						self.explorers_level_3_strategy['two_groups'] = gt32[0][1] + 0.0
						self.explorers_level_3_strategy['three_groups'] = gt32[0][2] + 0.0
						self.monsters_level_3_strategy['independent'] = gt32[0][3] + 0.0
						self.monsters_level_3_strategy['dependent'] = gt32[0][4] + 0.0

				elif self.explorers_level_2_strategy['nearest'] == 1 and self.monsters_level_2_strategy['highest_attacking_ability'] == 1:
					tmp_e = self.explorers_list
					tmp_m = self.monsters_list

					tmp_e.sort(key = lambda kk:(kk.get('energy_level', 0)), reverse = True)
					tmp_m.sort(key = lambda kk:(kk.get('adversary_distance', 0)))

					ee = tmp_e[0]['energy_level']

					the_explorer_attacking_level = self.explorer_attacking_ability(ee)
					the_explorer_defending_level = self.explorer_defending_ability(ee)

					em = tmp_m[0]['energy_level']

					the_monster_attacking_level = self.monster_attacking_ability(em)
					the_monster_defending_level = self.monster_defending_ability(em)

					gt33 = self.lowest_level_utility(the_explorer_attacking_level, the_monster_attacking_level, the_explorer_defending_level, the_monster_defending_level, num_explorers, num_monsters, explorer_area, monster_area)

					for i in range(len(gt33[0])):
						self.explorers_level_3_strategy['one_group'] = gt33[0][0] + 0.0
						self.explorers_level_3_strategy['two_groups'] = gt33[0][1] + 0.0
						self.explorers_level_3_strategy['three_groups'] = gt33[0][2] + 0.0
						self.monsters_level_3_strategy['independent'] = gt33[0][3] + 0.0
						self.monsters_level_3_strategy['dependent'] = gt33[0][4] + 0.0

				elif self.explorers_level_2_strategy['lowest_attacking_ability'] == 1 and self.monsters_level_2_strategy['highest_attacking_ability'] == 1:

					tmp_e = self.explorers_list
					tmp_m = self.monsters_list

					tmp_e.sort(key = lambda kk:(kk.get('energy_level', 0)), reverse = True)
					tmp_m.sort(key = lambda kk:(kk.get('energy_level', 0)))

					ee = tmp_e[0]['energy_level']

					the_explorer_attacking_level = self.explorer_attacking_ability(ee)
					the_explorer_defending_level = self.explorer_defending_ability(ee)

					em = tmp_m[0]['energy_level']

					the_monster_attacking_level = self.monster_attacking_ability(em)
					the_monster_defending_level = self.monster_defending_ability(em)

					gt34 = self.lowest_level_utility(the_explorer_attacking_level, the_monster_attacking_level, the_explorer_defending_level, the_monster_defending_level, num_explorers, num_monsters, explorer_area, monster_area)

					for i in range(len(gt34[0])):
						self.explorers_level_3_strategy['one_group'] = gt34[0][0] + 0.0
						self.explorers_level_3_strategy['two_groups'] = gt34[0][1] + 0.0
						self.explorers_level_3_strategy['three_groups'] = gt34[0][2] + 0.0
						self.monsters_level_3_strategy['independent'] = gt34[0][3] + 0.0
						self.monsters_level_3_strategy['dependent'] = gt34[0][4] + 0.0

				else:
					print("E_attack-M_attack strategy has some problem!")

			elif self.explorers_level_1_strategy['defend'] == 1 and self.monsters_level_1_strategy['defend'] == 1:
				tmpu_e = []

				for i in range(len(self.explorers_list)):
					tmpu_e.append(self.explorers_list[i]['unit_attacking_energy_cost'])

				explorer_average_UAEC = np.mean(tmpu_e)

				tmpn_m = self.monsters_list
				tmph_m = self.monsters_list

				tmpn_m.sort(key = lambda kk:(kk.get('adversary_distance', 0)))
				tmph_m.sort(key = lambda kk:(kk.get('energy_level', 0)), reverse = True)

				nearest_monster = tmpn_m[0]['unit_attacking_energy_cost']
				highest_attacking_ability_monster = tmph_m[0]['unit_attacking_energy_cost']

				m4 = np.array([[int(self.expected_enery_utility(explorer_average_AD/2, agent_velocity, num_explorers, num_monsters, explorer_average_UAEC, nearest_monster, explorer_area, monster_area)), \
								int(self.expected_enery_utility(explorer_average_AD/2, agent_velocity, num_explorers, num_monsters, 7 * explorer_average_UAEC/6, nearest_monster, explorer_area, monster_area))], \
							   [int(self.expected_enery_utility(explorer_average_AD/2, agent_velocity, num_explorers, num_monsters, explorer_average_UAEC, highest_attacking_ability_monster, explorer_area, monster_area)), \
							    int(self.expected_enery_utility(explorer_average_AD/2, agent_velocity, num_explorers, num_monsters, 7 * explorer_average_UAEC/6, highest_attacking_ability_monster, explorer_area, monster_area))]], dtype = gambit.Rational)

				g4 = gambit.Game.from_arrays(m4, -m4)

				z4 = gambit.nash.enummixed_solve(g4)

				for i in range(len(z4[0])):
					self.explorers_level_2_strategy['nearest'] = z4[0][0] + 0.0
					self.explorers_level_2_strategy['highest_attacking_ability'] = z4[0][1] + 0.0
					self.monsters_level_2_strategy['nearest'] = z4[0][2] + 0.0
					self.monsters_level_2_strategy['highest_attacking_ability'] = z4[0][3] + 0.0

				if self.explorers_level_2_strategy['nearest'] == 1 and self.monsters_level_2_strategy['nearest'] == 1:

					tmp_e = self.explorers_list
					tmp_m = self.monsters_list

					tmp_e.sort(key = lambda kk:(kk.get('adversary_distance', 0)))
					tmp_m.sort(key = lambda kk:(kk.get('adversary_distance', 0)))

					ee = tmp_e[0]['energy_level']

					the_explorer_attacking_level = self.explorer_attacking_ability(ee)
					the_explorer_defending_level = self.explorer_defending_ability(ee)

					em = tmp_m[0]['energy_level']

					the_monster_attacking_level = self.monster_attacking_ability(em)
					the_monster_defending_level = self.monster_defending_ability(em)

					gt41 = self.lowest_level_utility(the_explorer_attacking_level, the_monster_attacking_level, the_explorer_defending_level, the_monster_defending_level, num_explorers, num_monsters, explorer_area, monster_area)

					for i in range(len(gt41[0])):
						self.explorers_level_3_strategy['one_group'] = gt41[0][0] + 0.0
						self.explorers_level_3_strategy['two_groups'] = gt41[0][1] + 0.0
						self.explorers_level_3_strategy['three_groups'] = gt41[0][2] + 0.0
						self.monsters_level_3_strategy['independent'] = gt41[0][3] + 0.0
						self.monsters_level_3_strategy['dependent'] = gt41[0][4] + 0.0

				elif self.explorers_level_2_strategy['highest_attacking_ability'] == 1 and self.monsters_level_2_strategy['nearest'] == 1:

					tmp_e = self.explorers_list
					tmp_m = self.monsters_list

					tmp_e.sort(key = lambda kk:(kk.get('adversary_distance', 0)))
					tmp_m.sort(key = lambda kk:(kk.get('energy_level', 0)), reverse = True)

					ee = tmp_e[0]['energy_level']

					the_explorer_attacking_level = self.explorer_attacking_ability(ee)
					the_explorer_defending_level = self.explorer_defending_ability(ee)

					em = tmp_m[0]['energy_level']

					the_monster_attacking_level = self.monster_attacking_ability(em)
					the_monster_defending_level = self.monster_defending_ability(em)

					gt42 = self.lowest_level_utility(the_explorer_attacking_level, the_monster_attacking_level, the_explorer_defending_level, the_monster_defending_level, num_explorers, num_monsters, explorer_area, monster_area)

					for i in range(len(gt42[0])):
						self.explorers_level_3_strategy['one_group'] = gt42[0][0] + 0.0
						self.explorers_level_3_strategy['two_groups'] = gt42[0][1] + 0.0
						self.explorers_level_3_strategy['three_groups'] = gt42[0][2] + 0.0
						self.monsters_level_3_strategy['independent'] = gt42[0][3] + 0.0
						self.monsters_level_3_strategy['dependent'] = gt42[0][4] + 0.0

				elif self.explorers_level_2_strategy['nearest'] == 1 and self.monsters_level_2_strategy['highest_attacking_ability'] == 1:

					tmp_e = self.explorers_list
					tmp_m = self.monsters_list

					tmp_e.sort(key = lambda kk:(kk.get('energy_level', 0)), reverse = True)
					tmp_m.sort(key = lambda kk:(kk.get('adversary_distance', 0)))

					ee = tmp_e[0]['energy_level']

					the_explorer_attacking_level = self.explorer_attacking_ability(ee)
					the_explorer_defending_level = self.explorer_defending_ability(ee)

					em = tmp_m[0]['energy_level']

					the_monster_attacking_level = self.monster_attacking_ability(em)
					the_monster_defending_level = self.monster_defending_ability(em)

					gt43 = self.self.lowest_level_utility(the_explorer_attacking_level, the_monster_attacking_level, the_explorer_defending_level, the_monster_defending_level, num_explorers, num_monsters, explorer_area, monster_area)

					for i in range(len(gt43[0])):
						self.explorers_level_3_strategy['one_group'] = gt43[0][0] + 0.0
						self.explorers_level_3_strategy['two_groups'] = gt43[0][1] + 0.0
						self.explorers_level_3_strategy['three_groups'] = gt43[0][2] + 0.0
						self.monsters_level_3_strategy['independent'] = gt43[0][3] + 0.0
						self.monsters_level_3_strategy['dependent'] = gt43[0][4] + 0.0

				elif self.explorers_level_2_strategy['highest_attacking_ability'] == 1 and self.monsters_level_2_strategy['highest_attacking_ability'] == 1:

					tmp_e = self.explorers_list
					tmp_m = self.monsters_list

					tmp_e.sort(key = lambda kk:(kk.get('energy_level', 0)), reverse = True)
					tmp_m.sort(key = lambda kk:(kk.get('energy_level', 0)), reverse = True)

					ee = tmp_e[0]['energy_level']

					the_explorer_attacking_level = self.explorer_attacking_ability(ee)
					the_explorer_defending_level = self.explorer_defending_ability(ee)

					em = tmp_m[0]['energy_level']

					the_monster_attacking_level = self.monster_attacking_ability(em)
					the_monster_defending_level = self.monster_defending_ability(em)

					gt44 = self.lowest_level_utility(the_explorer_attacking_level, the_monster_attacking_level, the_explorer_defending_level, the_monster_defending_level, num_explorers, num_monsters, explorer_area, monster_area)

					for i in range(len(gt44[0])):
						self.explorers_level_3_strategy['one_group'] = gt44[0][0] + 0.0
						self.explorers_level_3_strategy['two_groups'] = gt44[0][1] + 0.0
						self.explorers_level_3_strategy['three_groups'] = gt44[0][2] + 0.0
						self.monsters_level_3_strategy['independent'] = gt44[0][3] + 0.0
						self.monsters_level_3_strategy['dependent'] = gt44[0][4] + 0.0

				else:
					print("E_attack-M_attack strategy has some problem!")

			else:
				print("Game level 1 has some problem!")

			tmpv_e = []
			tmpd_m = []
			tmpu_e = []
			tmpl_m = []
			tmph_m = []
			tmpn_m = []
			tmp_e  = []
			tmp_m  = []
			tmpa_e = []

		else:
			print("explorers_list has some problem!")

		print("Level 1 explorers' result is: ", self.explorers_level_1_strategy)
		print("Level 1 monsters' result is: ", self.monsters_level_1_strategy)

		print("Level 2 explorers' result is: ", self.explorers_level_2_strategy)
		print("Level 2 monsters' result is: ", self.monsters_level_2_strategy)

		print("Level 3 explorers' result is: ", self.explorers_level_3_strategy)
		print("Level 3 monsters' result is: ", self.monsters_level_3_strategy)

		file_object3 = open("explorersDecision.json", 'w+')
		file_object4 = open("monstersDecision.json",  'w+')

		# file_object3.write(str(self.explorers_level_1_strategy) + "\n" + \
		# 				   str(self.explorers_level_2_strategy) + "\n" + \
		# 				   str(self.explorers_level_3_strategy))

		# file_object4.write(str(self.monsters_level_1_strategy) + "\n" + \
		# 				   str(self.monsters_level_2_strategy) + "\n" + \
		# 				   str(self.monsters_level_3_strategy))

		# file_object3.write("{" + "level_1_decision" + ":" + "[" + json.dumps(self.explorers_level_1_strategy) + "],\n" + \
		# 				   "level_2_decision" + ":" + "[" + json.dumps(self.explorers_level_2_strategy) + "],\n" + \
		# 				   "level_3_decision" + ":" + "[" + json.dumps(self.explorers_level_3_strategy) + "]" + "}")

		file_object3.write(json.dumps({'level_1_decision':self.explorers_level_1_strategy, \
									   'level_2_decision':self.explorers_level_2_strategy, \
									   'level_3_decision':self.explorers_level_3_strategy}))

		# file_object4.write([json.dumps(self.monsters_level_1_strategy) + ",\n" + \
		# 				   json.dumps(self.monsters_level_2_strategy) + ",\n" + \
		# 				   json.dumps(self.monsters_level_3_strategy)])

		file_object3.close()
		file_object4.close()

if __name__ == '__main__':

	name = ['unit_attacking_energy_cost', 'energy_level', 'health_power', 'agent_area', 'agent_velocity', 'adversary_distance']
	explorers_list = []
	monsters_list = []
	file_object1 = open("explorersList.txt",'r')
	file_object2 = open("monstersList.txt",'r')

	try:
		lines1 = file_object1.readlines()
		lines2 = file_object2.readlines()

		for line in lines1:
			odom = line.split(",")
			numbers_float = map(float, odom)
			explorers_dic = dict(zip(name, numbers_float))
			explorers_list.append(explorers_dic)

		for line in lines2:
			odom = line.split(",")
			numbers_float = map(float, odom)
			monsters_dic = dict(zip(name, numbers_float))
			monsters_list.append(monsters_dic)

	finally:
	    file_object1.close()
	    file_object2.close()

	decision = Decision_making(explorers_list, monsters_list)

	decision.nash_equilibrium()

