#(c) 2013 btsekov@gmail.com

#election mandate distrbution, based on Hare-Niemeyer method 

#more info at http://en.wikipedia.org/wiki/Largest_remainder_method
# and http://electionscontest.files.wordpress.com

import os
import os.path
import sys

DEBUG = False

mirs = {}
parties = {}
votes = []
candidates = []
lots = []
result = {}

MINFILTER = 4.0

def get_fpath(dname, fname):
    if dname and len(dname) > 0:
        return dname + os.sep + fname
    return fname

def read_data(from_dir):
    f = open(get_fpath(from_dir, 'MIRs.txt'))
    data =  f.readlines()
    for row in data:
        csv = row.split(';')
        mirs[csv[0]] = dict(name=csv[1], mandate=int(csv[2]))

    f = open(get_fpath(from_dir, 'Parties.txt'))
    data =  f.readlines()
    for row in data:
        csv = row.split(';')
        parties[csv[0]] = dict(name=csv[1])

    f = open(get_fpath(from_dir, 'Votes.txt'))
    data =  f.readlines()
    for row in data:
        csv = row.split(';')
        votes.append(dict(mir=csv[0], party=csv[1], num=int(csv[2])))

    f = open(get_fpath(from_dir, 'Candidates.txt'))
    data =  f.readlines()
    for row in data:
        csv = row.split(';')
        candidates.append(dict(mir=csv[0], party=csv[1], num= int(csv[2]), name=csv[3]))

    if os.path.isfile(get_fpath(from_dir, 'Lot.txt')):
        f = open(get_fpath(from_dir, 'Lot.txt'))
        data =  f.readlines()
        for row in data:
            csv = row.split(';')
            lots.append(csv[0].replace('\n', ''))
    
def abort_file(message):
    f = open('Result.txt', 'w')
    f.write('0\n')
    f.write(message+'\n')
    f.close()
    sys.exit(2)    

def hare_quota(votes, mandates):
    return div5(votes, mandates)

def div5 (num, denom):
    return round(num/float(denom), 5)

def get_votes(party=None, mir=None):
    return extract_data(votes, 'num', party, mir)

def extract_data(data, field='num', party=None, mir=None):
    if (party == None and mir == None):
        return data[:]
    reduce_flag = False
    if (party != None and mir != None):
        reduce_flag = True
    
    res = []
    if reduce_flag:
        for x in data:
            if x['party'] == party and x['mir'] == mir:
                res.append(dict(m=x['mir'], num=x[field]))
    else:
        for x in data:
            if party:
                if x['party'] == party:
                    res.append(dict(m=x['mir'], num=x[field]))
            if mir:
                if x['mir'] == mir:
                    res.append(dict(p=x['party'], num=x[field]))
    return res            

def sum_num(data, field='num'):
    res = 0
    if isinstance(data, list):
        res = sum(x[field] for x in data)
    if isinstance(data, dict):
        res = sum(v[field] for k, v in data.items())
    return res

def alloc_mandates():
    tmp_alloc = []
    for mir in mirs.keys():
        n = sum_num(get_votes(mir=mir))
        d = mirs[mir]['mandate']
        if d > 0:
            quota = hare_quota(n, d)
            for p in parties.keys():
                f = div5(sum_num(get_votes(mir=mir, party=p)), quota)
                tmp_alloc.append(dict(mir=mir, party=p, init=int(f), quot = f-int(f), all=f))

    mandates = {}
    for k in parties.keys():
        t = sum_num(extract_data(tmp_alloc, 'init', party=k))
        mandates[k] = t
    stat = {}
    for mir in mirs.keys():
        if mirs[mir]['mandate'] > 0:
            data = extract_data(tmp_alloc, 'quot', mir=mir)
            remaining = int(mirs[mir]['mandate']-sum_num(extract_data(tmp_alloc, 'init', mir=mir)))
            if remaining > 0:
                #sort
                tmp2=sorted(data, reverse=True, key=lambda x: x['num'])
                stat.setdefault(mir, {})
                stat[mir]['all'] = tmp2[:]
                stat[mir]['free'] = tmp2[remaining:]
                stat[mir]['used'] = tmp2[:remaining]

                #check if Lot is needed to continue
                c=0
                for i in range(remaining):
                    if abs(tmp2[i]['num'] - tmp2[0]['num']) < 0.0000001:
                        c += 1
                if (c-1) > remaining:
                    abort_file('new Lot is needed.')
                #allocate additional mandtae to party
                for i in range(remaining):
                    x = tmp2[i]
                    mandates[x['p']] += 1
    return dict(alloc=tmp_alloc, mandates=mandates, usage=stat)

def test_allocation(allocated, target):
    s = 0
    res = {}
    for k in target.keys():
        diff = target[k] - allocated[k]
        s +=  abs(diff)
        res[k] = diff
    return (s, res)    

def collect_results():
    m = m1['usage'].keys()
    m = sorted(m)

    p = parties.keys()
    p = sorted(p)      

    for mir in m:
        result.setdefault(mir, {})
        t = m1['usage'][mir]['used']
        for party in p:
            for x in t:
                if x['p'] == party:
                    result[mir].setdefault(party, 0)
                    result[mir][party] += 1
                    if DEBUG:
                        print 'Mandate in mir "%s" for party "%s".' % (mir, party)
            
            buf = extract_data(m1['alloc'], 'init', mir=mir, party=party)
            for i in range(buf[0]['num']):
                result[mir].setdefault(party, 0)
                result[mir][party] += 1
                if DEBUG:
                    print 'Mandate in mir "%s" for party "%s"+' % (mir, party)

def dump_results():
    #dump results
    m = m1['usage'].keys()
    m = sorted(m)

    p = parties.keys()
    p = sorted(p)      

    for mir in m:
        t = m1['usage'][mir]['used']
        for party in p:
            for x in t:
                if x['p'] == party:
                    print 'Mandate in mir "%s" for party "%s".' % (mir, party)
            
            buf = extract_data(m1['alloc'], 'init', mir=mir, party=party)
            for i in range(buf[0]['num']):
                print 'Mandate in mir "%s" for party "%s"+' % (mir, party)
    

if __name__ == '__main__':

    if len(sys.argv) < 3:
        print 'Usage: btsekov_pe2013.py <directory> <minimum % votes>'
        print 'where'
        print'    <directory> points to a folder that contains data for Elections Contest.'
        print'    <minimum % votes> define the minimum % votes that every party must receive.'

        if os.path.isfile('MIRs.txt'):
            print'='*45
            print 'Using default values: current dir and 4.0%...'
            print'='*45
            read_data(None)
            MINFILTER = float(4.0)
        else:
            sys.exit(0)        
    else:
        read_data(sys.argv[1])
        MINFILTER = float(sys.argv[2])
    
    all_mandates = sum_num(mirs, 'mandate')
    all_votes = sum_num(votes)
    if DEBUG:
        print 'Initial data: using total %s mandates and %d valid votes.' % (all_mandates, all_votes)

    #filter out all parties below the minimum barrier - MINFILTER %s of all votes
    min_votes = round(all_votes* MINFILTER/100., 1)
    removed_parties=[]
    for p in parties.keys():
        party_votes = sum_num(extract_data(votes, party=p))
        if party_votes  < min_votes:
            if DEBUG:            
                print ' [-] Removing party "%s", as its vots are below minimum %% (%.2f) (%d votes)' % (p, MINFILTER, party_votes)
            del parties[p]
            removed_parties.append(p)

    zz=votes[:]
    votes=[]
    for x in zz:
        if x['party'] not in removed_parties:
            votes.append(x)    
    
    all_mandates = sum_num(mirs, 'mandate')
    all_votes = sum_num(votes)
    if DEBUG:
        print 'After filtering: Using total %s mandates and %d valid votes.' % (all_mandates, all_votes)

    #initial allocation of mandates to all parties
    hare_all = hare_quota(all_votes, all_mandates)
    target_mandates = {} #keys: init, quot, all
    for p in parties.keys():
        f = div5(sum_num(extract_data(votes, party=p)), hare_all)
        target_mandates[p] = dict(init=int(f), quot = f-int(f), all=int(f))

    #distribute remaining mandates to parties
    remaining = all_mandates - sum(x['init'] for x in target_mandates.values())
    if remaining > 0:
        #sort
        tmp=[(v['quot'], k) for k, v in target_mandates.items()]
        tmp=sorted(tmp, reverse=True, key=lambda x: x[0])
##        if len(tmp) < remaining:
##            print tmp
##            print remaining
##            sys.exit(2)
        for i in range(remaining):
            x = tmp[i]
            target_mandates[x[1]]['all'] += 1
                    
    #try to allocate all mandates from mirs, so that data in target_mandates[all] will be preserved                    

##    sum_num(votes)
##    sum_num(get_votes(party='1'))
##    hare_quota(sum_num(get_votes(mir='1')), mirs['1']['mandate'])
##    

    m1=alloc_mandates()
    if DEBUG:
        print 'allocated mandates: ', m1['mandates']
    tm = {k:v['all'] for k, v in target_mandates.items()}
    if DEBUG:
        print 'target mandates: ', tm
        print 'Adjustments to currently allocated mandates:'
    s = 0
    for k in m1['mandates'].keys():
        s +=  abs(tm[k] - m1['mandates'][k])
        if DEBUG:
            print '  mandate %s: %d' % (k, tm[k] - m1['mandates'][k])
    if s == 0:
        if DEBUG:
            print 'No need of further adjustments.'        

    #main loop to perform adjustmetns of allocation of mandates
    #stops when diff=0
    #or abort if not possible to move mandates to more appropirate candidate    

    count = 0
    while 1:
        (diff, alloc_map)=test_allocation(tm, m1['mandates'])
        if diff == 0 or count > 1000:
            break
        
        b = m1['usage']
        buf = []
        #get all extra allocated mandates and selects lowest quot
        for k, v in alloc_map.items():
            if v > 0:
                for mir in b.keys():
                    t = b[mir]['used']
                    for x in t:
                        if x['p'] == k:
                            new1 = x
                            new1['m']=mir
                            buf.append(new1)
    #                        print new1

        tmp2=sorted(buf, key=lambda x: x['num'])
        selected = tmp2[0]
        sel_mir = selected['m']
        sel_party  =selected['p']
        if len(m1['usage'][sel_mir]['free']) < 1:
            abort_file('Unable to allocate new mandate during adjustment phase.')
            sys.exit(2)
        #remove selected
        m1['usage'][sel_mir]['used'].remove(selected)
        m1['mandates'][sel_party] -= 1
        if DEBUG:
            print 'Removing mandate for party "%s" in mir "%s"' % (sel_party, sel_mir)
        #allocate new mandate
        next_mandate = m1['usage'][sel_mir]['free'][0]
        m1['usage'][sel_mir]['free'].remove(next_mandate)
        m1['usage'][sel_mir]['used'].append(next_mandate)
        next_party = next_mandate['p']
        m1['mandates'][next_party] += 1
        if DEBUG:
            print 'Allocating new mandate for party "%s" in mir "%s"' % (next_party, sel_mir)
        count += 1
    if DEBUG:
        print 'Final allocation of mandates:'
        print m1['mandates']
    collect_results()
    #dump results
    mir = sorted(result.keys(), key=lambda x: int(x))
    f = open('Result.txt', 'wt')
    for m in mir:
        party = sorted(result[m].keys(), key=lambda x: int(x))
        for p in party:
            if result[m][p] > 0:
                #print '%s;%s;%s' % (m, p, result[m][p])
                s = '%s;%s;%s' % (m, p, result[m][p])
                f.write(s + '\n')

    f.close()   
