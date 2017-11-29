(ns Solution
  (:gen-class))

(defn -main [& args]
    
    (let [L (read) C (read) N (read)]
            (let [G (loop [i 0, res []]
                (if (< i N) (recur (inc i) (conj res (read))) res))]        
                
                (let [D (loop [i 0, res []]
                    (if (< i N)
                        (recur (inc i) (conj res 
                            (loop [nidx (rem (inc i) N), cnt (nth G i)]
                                (if (and (not= nidx i) (<= (+ cnt (nth G nidx)) L))
                                    (recur (rem (+ nidx 1) N) (+ cnt (nth G nidx)))
                                    [nidx,cnt]
                                )
                            ))
                        )
                        res)
                    )
                ]
                
                (let [T (loop [tc 0, tg 0, gi 0]
                    (if (< tc C)
                        (recur (inc tc) (+ tg (nth (nth D gi) 1)) (nth (nth D gi) 0))
                        tg
                    ))]
            
                    (println T)
                )
            ))
    )
)