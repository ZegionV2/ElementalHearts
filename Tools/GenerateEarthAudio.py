from pathlib import Path
import array,hashlib,json,math,random,wave,xml.etree.ElementTree as E
r=Path(__file__).resolve().parent.parent;rate=44100
out=r/'Docs/Audio';out.mkdir(parents=True,exist_ok=True)
def lowpass(xs,hz):
    a=1-math.exp(-2*math.pi*hz/rate);previous=0.;result=[]
    for x in xs:
        previous+=a*(x-previous);result.append(previous)
    return result
def render(kind,variant):
    rng=random.Random(73120+variant*331+['Advance','Charge','Quake','Spire'].index(kind)*1703)
    duration={'Advance':.52,'Charge':.57,'Quake':1.24,'Spire':1.02}[kind]
    n=int(rate*duration);noise=[rng.uniform(-1,1) for _ in range(n)]
    lows=lowpass(noise,220);sub=lowpass(noise,30);mid=lowpass(noise,1800);top=lowpass(noise,6200)
    chips=[0.]*n
    for _ in range({'Advance':14,'Charge':24,'Quake':18,'Spire':28}[kind]):
        at=rng.uniform(.03,duration*.82);amp=rng.uniform(.07,.22);length=rng.uniform(.015,.055)
        first=int(at*rate)
        for j in range(min(int(length*rate),n-first)):
            local=j/rate;chips[first+j]+=amp*math.exp(-local*95)*(top[first+j]-mid[first+j])
    samples=[];phase=0.
    for i in range(n):
        s=i/rate;f=s/duration
        rumble=(lows[i]-sub[i])*4.;grit=(mid[i]-lows[i])*1.3;crack=top[i]-mid[i]
        if kind=='Charge':
            value=(.38*grit+.28*rumble)*(.15+.85*f**1.7)*(1+.14*math.sin(s*63))+chips[i]*f
        elif kind=='Advance':
            value=(.47*grit+.5*rumble)*math.exp(-s*5.8)+.25*crack*math.exp(-s*60)+chips[i]
        else:
            frequency=(64 if kind=='Quake' else 91)*(1-.45*f)*(1+.025*variant)
            phase+=2*math.pi*frequency/rate
            value=(.30*math.sin(phase)*math.exp(-s*9)+.73*rumble*math.exp(-s*(3.6 if kind=='Quake' else 5.3))+.34*grit*math.exp(-s*8)+.40*crack*math.exp(-s*(58 if kind=='Quake' else 80))+chips[i])
        fadein=min(1.,s/.003);fadeout=min(1.,(duration-s)/.045)
        samples.append(value*fadein*fadeout)
    # Small, finite stone-space tail; no loop, sustain or runtime-generated audio.
    dry=samples[:]
    for delay,gain in [(0.031,.13),(.069,.075),(.113,.04)]:
        d=int(delay*rate)
        for i in range(d,n):samples[i]+=dry[i-d]*gain
    dc=lowpass(samples,24);samples=[x-y for x,y in zip(samples,dc)]
    peak=max(abs(x) for x in samples);scale=.76/max(peak,1e-9)
    data=array.array('h',(round(max(-.999,min(.999,x*scale))*32767) for x in samples))
    folder=r/'Sounds/ElementalHearts/Earth'/kind;folder.mkdir(parents=True,exist_ok=True);p=folder/f'{kind}_{variant:02}.wav'
    with wave.open(str(p),'wb') as w:w.setparams((1,2,rate,0,'NONE','not compressed'));w.writeframes(data.tobytes())
    return {'file':p.relative_to(r).as_posix(),'duration':duration,'rate':rate,'channels':1,'peak':max(abs(x) for x in data)/32768.,'rms':math.sqrt(sum(x*x for x in data)/len(data))/32768.,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()}
clips=[render(k,v) for k in ['Advance','Charge','Quake','Spire'] for v in [1,2]]
(out/'EarthSupportSamples.json').write_text(json.dumps(clips,indent=2))
print('Regenerated eight original Earth support clips; Core sounds remain runtime references.')
