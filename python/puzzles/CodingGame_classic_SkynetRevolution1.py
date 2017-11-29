import sys
import math

n, l, e = [int(i) for i in input().split()]

def debug(msg):
    print(msg, file=sys.stderr)

class Node:
    def __init__(self):
        self.idx=-1
        self.children=[]
        self.isExit=False
        self.isUsed=False
        
nodes=[None]*n # children, move count, is exit, used

for i in range(n):
    debug("Create node " + str(i))
    nodes[i]=Node()
    nodes[i].idx=i

for i in range(l):
    n1, n2 = [int(j) for j in input().split()]
    debug("Link node "+str(n1)+" and "+str(n2))
    nodes[n1].children.append(n2)
    nodes[n2].children.append(n1)

for i in range(e):
    ei = int(input())  # the index of a gateway node
    nodes[ei].isExit=True
    debug("Node " + str(ei) + " is exit !")

# game loop
while True:
    si = int(input())  # The index of the node on which the Skynet agent is positioned this turn
    
    id1=-1
    id2=-1

    for child in nodes[si].children:
        if nodes[child].isExit:
            id1=si
            id2=child
            
    if id1==-1:
        for node in nodes:
            node.isUsed = False
            node.moveCount = -1
        
        nodes[si].moveCount=0
        nodes[si].isUsed=True
        
        bestNextNode = None
    
        toCheck = []
        
        for childIdx in nodes[si].children:
            toCheck.append((None, nodes[childIdx]))
        
        while bestNextNode is None:
            
            newToCheck = []
            
            for info in toCheck:
                if info[1].isExit:
                    bestNextNode = info[0]
                    break;
                
                info[1].isUsed = True
                
                for childIdx in info[1].children:
                    if not nodes[childIdx].isUsed:
                        if info[0] is None:
                            newToCheck.append((info[1], nodes[childIdx]))
                        else:
                            newToCheck.append((info[0], nodes[childIdx])) 
                            
                toCheck = newToCheck

        id1=si
        id2=bestNextNode.idx
            
    nodes[id1].children.remove(id2)
    nodes[id2].children.remove(id1)
    
    print(str(id1) + " " + str(id2))