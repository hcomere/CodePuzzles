import sys
import math
import datetime as dt

print("Reading input ...")
n1=dt.datetime.now()

l, c, n = [int(i) for i in input().split()]

G = [0] * n

for i in range(n):
    G[i] = int(input())


n2=dt.datetime.now()
print("Elapsed time : %.3f ms" % ((n2-n1).microseconds / 1000))


print("Computing sub solutions ...")
n1=dt.datetime.now()

next = [0] * n
gain = [0] * n

for i in range(0, n) :
    next[i] = (i+1)%n
    gain[i] = G[i]
    count = G[i]

    while (next[i] != i) and (count + G[next[i]] <= l):
        count += G[next[i]]
        next[i] = (next[i]+1) % n
        
    gain[i] = count
    
n2=dt.datetime.now()
print("Elapsed time : %.3f ms" % ((n2-n1).microseconds / 1000))
   
   
print("Computing final gain ...")
n1=dt.datetime.now()

turnCount = 0
totalGain = 0
groupIndex = 0

while turnCount < c:
    totalGain += gain[groupIndex]
    groupIndex = next[groupIndex]
    turnCount += 1

n2=dt.datetime.now()
print("Elapsed time : %.3f ms" % ((n2-n1).microseconds / 1000))

print(totalGain)