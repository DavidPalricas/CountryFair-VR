

Universidade de Aveiro
## 2026
## David
## Freitas Palricas
Explorando os Efeitos de Jogos Sérios de
Realidade Virtual Personalizada para
Reabilitação de Acidente Vascular Cerebral
Exploring the Effect of Personalized Virtual
Reality Serious Game for Stroke Rehabilitation

Universidade de Aveiro
## 2026
## David
## Freitas Palricas
Explorando os Efeitos de Jogos Sérios de
Realidade Virtual Personalizada para
Reabilitação de Acidente Vascular Cerebral
Exploring the Effect of Personalized Virtual
Reality Serious Game for Stroke Rehabilitation
Dissertação apresentada à Universidade de Aveiro para cumprimento
dos  requisitos  necessários  à  obtenção  do  grau  de  Mestre  em  De-
senvolvimento de Jogos Digitais, realizado sob a orientação científica
do Doutor Bernardo Marques, Professor auxiliar do Departamento de
Eletrónica, Telecomunicações e Informática da Universidade de Aveiro,
da Doutora Beatriz Sousa Santos, Professora catedrática  do Departa-
mento de Eletrónica, Telecomunicações e Informática da Universidade
de Aveiro.

presidente / presidentProfessor Doutor Mário Jorge Rodrigues Martins Vairinhos
Professor Associado da Universidade de Aveiro
vogais / examiners committeeDoutor João Tiago Pinheiro Neto Jacob
Technical Product Owner /Tech Lead da Mindera Portugal
## Professor Doutor Bernardo José Santos Marques
Professor Auxiliar em Regime Laboral da Universidade de Aveiro

agradecimentos /
acknowledgements
This work would not have been possible without the support of many
people, to whom I am deeply grateful.
First, I would like to thank my supervisors, Professor Bernardo Marques
and Professor Beatriz Sousa Santos, for their guidance, availability and
encouragement throughout this journey. Their expertise and critical eye
were essential in shaping both the research and this dissertation.
We thank everyone involved for their time and expertise, in particular
the stroke survivors and healthcare professionals from the Rovisco Pais
rehabilitation center.
I would also like to thank all the participants who took part in the user
studies.  Their availability and feedback were decisive in validating and
improving the system.
A special thanks to Ana Rita, whose contribution to the Archery mini-
game was fundamental to the project, and to Bianca and Inês for their
involvement and support throughout the development process.
This   work   was   funded   by   the   European   Regional   Development
Fund  (FEDER)  under  the  COMPETE  2030  /  Portugal  2030  Pro-
gram   (COMPETE2030-FEDER-00815600/#16874),   and   was   sup-
ported   by   the   Portuguese   Foundation   for   Science   and   Technol-
ogy   (FCT)   under   the   IC&DT   2023   Call,   and   through   contract
doi.org/10.54499/UID/00127/2025 - IEETA
Finally, I would like to express my gratitude to my family and friends,
whose  encouragement  and  patience  sustained  me  throughout  this
work.

palavras-chaveRealidade Virtual, Jogos Sérios, Personalização, Design Centrado no
## Utilizador, Acidente Vascular Cerebral, Reabilitação
resumoO  Acidente  Vascular  Cerebral  (AVC)  é  uma  das  principais  causas
de  incapacidade  de  longa  duração  a  nível  mundial,  comprometendo
progressivamente  as  funções  motoras,  cognitivas  e  a  qualidade  de
vida dos sobreviventes.  Os programas de reabilitação convencionais
apresentam  duas  limitações  críticas:    a  monotonia  dos  exercícios
terapêuticos  repetitivos  e  a  ausência  de  adaptação  individualizada
fatores  que  corrompem  a  motivação  e  o  envolvimento  do  paciente.
Esta dissertação apresenta o Country Fair VR, um protótipo de jogo
sério  em  Realidade  Virtual  para  o  Meta  Quest  3,  concebido  para
apoiar a reabilitação dos membros superiores e das funções cognitivas
de  sobreviventes  de  AVC.  O  sistema  assenta  num  framework  de
personalização em closed-loop com um motor de Ajuste Dinâmico de
Dificuldade  baseado  em  regras,  implementando  um  modelo  híbrido
que combina adaptação automática em tempo real com a capacidade
de sobreposição manual pelo terapeuta, preferência validada por 70%
dos  profissionais  de  saúde  inquiridos  no  Centro  de  Reabilitação  de
Rovisco  Pais.O  protótipo  inclui  dois  mini-jogos  terapeuticamente
fundamentados numa feria popular virtual:  um jogo de Tiro com Arco,
focado na força bimanual, treino propriocetivo e atenção seletiva, e um
jogo de Frisbee, direcionado para a extensão do cotovelo, coordenação
do  pulso,  integração  visuoespacial  e  capacidade  de  reação.Foi
ainda  integrado  um  Módulo  de  Reconhecimento  de  Emoções  como
prova  de  conceito,  transmitindo  o  estado  emocional  do  sobrevivente
para  o  ambiente  VR  de  outro  utilizador  para  mitigar  o  isolamento
social.Avaliado  através  de  um  estudo  de  utilizadores  em  duas
fases  (N  =  38),  o  protótipo  obteve  uma  pontuação  média  de  82/100
na  SUS  (Excelente),  com  82%  dos  participantes  a  percecionarem  a
dificuldade  adaptativa  em  ação,  98%  a  reportarem  uma  experiência
motivante,  e  68%  a  solicitarem  espontaneamente  continuar  a  jogar
após a sessão estabelecendo uma base sólida para futura validação
clínica longitudinal.

keywordsVirtual Reality,  Serious Game,  Personalization User-centered design,
## Stroke, Rehabilitation
abstractStroke is one of the leading causes of long-term disability worldwide,
progressively eroding survivors’ motor function, cognition, and quality
of  life.    Conventional  rehabilitation  programs  suffer  from  two  critical
limitations: monotony arising from repetitive therapeutic exercises, and
a  lack  of  individualized  adaptation  both  of  which  undermine  patient
motivation and engagement.   This dissertation presents Country Fair
VR,  a  Virtual  Reality  serious  game  prototype  for  the  Meta  Quest  3,
designed  to  support  upper  limb  and  cognitive  rehabilitation  of  stroke
survivors.   The  system  is  built  around  a  closed-loop  personalization
framework featuring a rule-based Dynamic Difficulty Adjustment (DDA)
engine implementing a hybrid model that combines autonomous real-
time adaptation with therapist manual override a preference validated
by  70%  of  healthcare  professionals  surveyed  at  the  Rovisco  Pais
Rehabilitation  Center.The  prototype  features  two  therapeutically
grounded  mini-games  set  in  a  vibrant  country  fair  environment:   an
Archery game targeting bimanual strength, proprioceptive training, and
selective  attention  and  a  Frisbee  game  targeting  elbow  extension,
wrist coordination, visuospatial integration, and reaction capability.  An
Emotion Recognition Module was additionally integrated as a proof-of-
concept, relaying survivors’ emotional states to peers’ VR environments
to mitigate social isolation.   Evaluated through a structured two-wave
user  study  (N  =  38),  the  prototype  achieved  a  mean  SUS  score
of  82/100  (Excellent),  with  82%  of  participants  perceiving  adaptive
difficulty  in  action,  98%  reporting  a  motivating  experience,  and  68%
spontaneously requesting to continue playing after the session ended
establishing a solid foundation for future longitudinal clinical validation.

acknowledgement of use of
AI tools
Recognition   of   the   use   of   generative   Artificial   Intelligence
technologies and tools, software and other support tools.
I   acknowledge   the   use   of   Claude   (Anthropic,    https://claude.ai)
for    text    translation    and    grammatical    correction,NotebookLM
(Google,   https://notebooklm.google.com)   to   verify   the   consistency
of  the  written  content  against  the  references  and  Gemini  (Google,
https://gemini.google.com/) for image generation.

## Contents
## Contentsi
List of Figuresiv
List of Tablesvii
## Glossaryviii
## 1  Introduction1
1.1  Goals and Contributions  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .    3
1.1.1   Thesis Structure  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .    4
2  Background and related work5
2.1  Stroke causes and challenges . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .    5
2.2  Digital Realities and Serious Games in Stroke Rehabilitation . . . . . . . . . . . . . .    7
2.3  Personalization Strategies in VR Rehabilitation  . . . . . . . . . . . . . . . . . . . . .    9
2.3.1   Taxonomy of Personalization  . . . . . . . . . . . . . . . . . . . . . . . . . . .    9
2.3.2   Automation vs Clinical Control . . . . . . . . . . . . . . . . . . . . . . . . . .   10
2.4  Dynamic Difficulty Adjustment (DDA) and Flow Theory . . . . . . . . . . . . . . . .   11
2.4.1   Mechanisms of Adaptation . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   12
2.5  Human-Centered Design and Clinical Acceptance  . . . . . . . . . . . . . . . . . . . .   13
2.5.1   Bridging the Gap: Role of Health Care Professionals . . . . . . . . . . . . . .   14
2.6  Summary . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   14
3  Personalized VR-based Country Fair16
3.1  Methodology . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   16
3.2  Architecture  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   17
3.3  Country Fair Model and Personalization Integration . . . . . . . . . . . . . . . . . . .   22
3.3.1   Personalization Hybrid Approach . . . . . . . . . . . . . . . . . . . . . . . . .   24
3.4  Mini-Games  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   29
3.4.1   Common Interactions  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   29
i

3.4.2   Archery Mini-Game  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   30
3.4.3   Frisbee Mini-Game . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   32
3.5  Emotion Recognition Module . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   35
3.6  Technologies and Implementation  . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   38
3.6.1   Hardware  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   38
3.6.2   Software . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   38
3.6.3   Visual Art Direction and References  . . . . . . . . . . . . . . . . . . . . . . .   39
## 4  User Study42
4.1  First Wave: Informal Usability Testing  . . . . . . . . . . . . . . . . . . . . . . . . . .   42
4.1.1   Prototype State . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   42
4.1.2   Experimental Setup  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   43
4.1.3   Measurements . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   44
4.1.4   Procedure . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   44
4.1.5   Results and Feedback  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   44
4.2  Second Wave: Formal Usability Testing Without the Target Audience . . . . . . . . .   45
4.2.1   Prototype State . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   45
4.2.2   Experimental Setup  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   47
4.2.3   Tasks . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   47
4.2.4   Measurements . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   48
4.2.5   Procedure . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   48
4.2.6   Participants . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   49
4.2.7   Data Analysis . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   50
4.2.8   Observations and Results  . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   50
4.2.9   Limitations  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   53
4.3  Parallel Study: Emotion Visualization Evaluation  . . . . . . . . . . . . . . . . . . . .   53
4.3.1   Prototype State . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   53
4.3.2   Results . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   54
4.4  Summary . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   55
5  Conclusion and Future Work56
5.1  Conclusion . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   56
5.2  Future Work . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   57
## References59
## A Additional Content62
A.1  Websites of Institutions . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   62
A.2  Online Resources  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   62
ii

A.3  Academic Courses . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   62
A.4  Country Fair VR Prototype Demo Videos . . . . . . . . . . . . . . . . . . . . . . . . .   62
A.5  Project Repository . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   62
A.6  Second Wave of User Tests Questionnaire . . . . . . . . . . . . . . . . . . . . . . . . .   63
A.7  Second Wave of User Tests System Usability Scale (SUS) BoxPlot . . . . . . . . . . .   69
A.8  Second Wave of User Tests SUS BarChart  . . . . . . . . . . . . . . . . . . . . . . . .   70
A.9  Second Wave of User Tests Tasks Completion BoxPlot  . . . . . . . . . . . . . . . . .   70
A.10 Second Wave of User Tests Tasks Completion BarCharts  . . . . . . . . . . . . . . . .   71
iii

List of Figures
## 2.1
Overview of the principal modifiable risk factors associated with stroke, including metabolic
conditions (obesity, diabetes, and hypercholesterolaemia), behavioural factors (physical
inactivity, smoking, and heavy alcohol use).
## 1
## . . . . . . . . . . . . . . . . . . . . . . . . .    6
2.2  Reality Virtuality Continuum, adapted from [15].  . . . . . . . . . . . . . . . . . . . . . .    7
2.3Distribution of publications across different personalization methods,highlighting Behavioral
tracking as the most studied approach, followed by Rule-based and Biofeedback methods.
Adapted from [3] . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   10
2.4Distribution of healthcare professionals votes for each personalization mode: hybrid (7),
manual (3), automatic (0).Adapted from [4] . . . . . . . . . . . . . . . . . . . . . . . . . .   11
2.5Mihaly Csikszentmihalyi’s Flow Theory: the Flow Zone is reached when challenge and
skill are in balance. Too much challenge produces anxiety, too little produces boredom.
Adapted from [30]. . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   12
2.6The Dual Flow Model for exergaming proposed by Sinclair et al. It extends Csikszentmi-
halyi’s single-axis flow with two orthogonal dimensions: an attractiveness axis, balancing
challenge against skill to sustain engagement, and an effectiveness axis, ensuring that
the physical intensity of play remains within a therapeutically beneficial range. Adapted
from [33]. . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   12
3.1Human-Centered Design (HCD) methodology adopted for the design and develop-
ment of the personalized Virtual Reality (VR) serious game, structured into iterative
phases spanning requirements extraction with stroke survivors and healthcare professionals,
prototyping, and waves of user testing.  . . . . . . . . . . . . . . . . . . . . . . . . . . . .   17
3.2Illustration of the research team conducting a user study, with a stroke survivor answering
a post-task questionnaire, with the support of the research members.  . . . . . . . . . . .   17
iv

3.3System architecture of Country Fair VR. The architecture contains three primary
modules: (1) a Personalization module, managed by the Carny Wise character, which
signals the Mini-Game Manager to dynamically adjust adaptive parameters and deliver
multimodal contextual feedback; (2) a Game module, structured around a Country Fair
Environment as the central hub, where the player can select between two distinct mini-
game environments, Archery and Frisbee, whose configuration can be modified by the
Personalization module through the adjustment of adaptive parameters. A Healthcare
Professional interacts with the system to configure and monitor patient sessions, while the
Stroke Survivor engages with the VR environment through a head-mounted display.  . .   18
3.4Conceptual framework for a closed-loopVRrehabilitation system combiningDDAand
hybrid personalization to sustain motivation and promote neuroplasticity in stroke survivors.
Adapted from [5].  . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   19
3.5The two mini-games integrated in the Country Fair VR system: the Archery mini-game
(left), where the player uses a virtual bow to pop balloons, and the Frisbee mini-game
(right), where the player throws a frisbee towards target areas for a virtual dog to retrieve.
## 2
## 20
3.6Zeca Bigodes dialogue panels: introduction to the fair (top), positive performance feedback
after completing the Frisbee mini-game (bottom left), and positive performance feedback
after completing the Archery mini-game (bottom right). . . . . . . . . . . . . . . . . . . .   23
## 3.7
Carny Wise tutorial sequence: instructional dialogue accompanied by a demonstrative
video showing the gesture to be performed, during the Frisbee mini-game tutorial (left)
and the Archery mini-game tutorial (right). . . . . . . . . . . . . . . . . . . . . . . . . . .   24
3.8Flowchart of the CarnyWiseDDAalgorithm, illustrating the two main adaptation branches:
the Failure Handling branch, which triggers a difficulty decrease upon four or more
consecutive misses, and the Precision Analysis branch, which accumulates Excel and
Struggle counters based on score precision thresholds (≥60% and≤40%, respectively) to
drive automatic difficulty adjustment. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   26
3.9Example of two Unity Animation Curves used in the Archery Mini Game: the left one
is used to configure the balloons that move, and the right one to configure the balloons
whose transparency changes. . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   28
3.10Interaction gestures used to interact with the game’s virtual environment: Poke gesture
(left) Grab gesture (right). Created by Gemini Pro.  . . . . . . . . . . . . . . . . . . . . .   29
3.11A participant performing the Archery mini-game gesture while seated, using the Meta
Quest 3 headset with hand-tracking interaction. . . . . . . . . . . . . . . . . . . . . . . .   31
## 3.12
Schematic representation of the forces acting on the mini-game props during their motion:
on the left, the arrow (archery mini-game) subject to launch force, gravity, and drag; on
the right, the frisbee (frisbee mini-game) subject to launch force, gravity, drag, and lift.
Created by Gemini Pro. . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   32
v

3.13A participant performing the Frisbee mini-game throwing gesture while seated, using the
Meta Quest 3 headset with hand-tracking interaction. . . . . . . . . . . . . . . . . . . .   34
3.14Evolution of the emotion slider across three representative states: negative (left), neutral
(center), and positive (right). Positive emotions detected shift the slider upward, negative
emotions shift it downward, and neutral emotions leave it unchanged.  The slider is
initialized at a neutral value. . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   36
3.15Illustration of the emotion recognition module: a laptop webcam captures the user’s facial
expressions (bottom-left), which are classified into one of the supported emotion categories
(top-left), and the resulting emotion is relayed to the stroke survivor’s VR mini-game
environment as an emoji overlay, exemplified here by a happy emotion in the Frisbee
mini-game and a neutral emotion in the Archery mini-game (right). . . . . . . . . . . . .   37
3.16Art direction reference board for the “Country Fair” environment.  Environmental
references (carnival tents, Ferris wheel, fairground props, and color palette) were gathered
from Pinterest using the keywords Carnival and Country Fair. Additionally, the cover
art and host character from the Carnival Games series (2K Play, Nintendo) informed the
stylized, cartoon-like design of the game’s Non-Playable Characters (NPCs), while the
disc dog activity from Wii Sports Resort (Nintendo, 2009) served as the primary gameplay
and visual reference for the Frisbee mini-game. . . . . . . . . . . . . . . . . . . . . . . . .   39
## 3.17
3D assets modelled and rigged by the author in Blender: theNPCs, comprising the
original character model sourced from Poly Pizza (left) and its modified version retextured
to adopt a carnival worker aesthetic (center-top), the frisbee model (right-top); the ambient
environment props, comprising trees, bushes, and rocks (bottom-left) and the dog model
with its full rig, used to drive the custom Idle and Walk animations developed for the
Frisbee mini-game (bottom-right). . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   41
4.1Screenshots of the prototype evaluated during the first wave of testing: the Country Fair
scene (top), the Archery mini-game (bottom left), and the Frisbee mini-game (bottom right).43
4.2First Wave informal usability session: two participants interacting with the Country Fair
VR prototype from a seated position using the Meta Quest 3 Head-Mounted Display (
## HMD)
and Sony WH-CH500 wireless headphones. . . . . . . . . . . . . . . . . . . . . . . . . . .   44
## 4.3
Screenshots of the prototype evaluated during the second wave of testing: the Country Fair
scene (top), the Archery mini-game (bottom left), and the Frisbee mini-game (bottom right).47
4.4Second Wave formal usability study: a representative sample of 15 of the 38 participants
interacting with the Country Fair VR prototype from a seated position, evaluating
system usability, perceptibility of theDDApersonalization system, and overall engagement
with the “Country Fair” environment. . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   49
vi

List of Tables
2.1  Summary of literature gaps and resulting design decisions.  . . . . . . . . . . . . . . . . .   15
3.1  Keyboard cheatcodes available to healthcare professionals during runtime. . . . . . . . . .   21
3.2  Adaptation Parameters per Minigame . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   28
3.3  Mini Games Score System . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . .   32
3.4All supported emotion types with their corresponding emojis, facial expression representa-
tions, and valence categories (Positive, Negative, or Neutral). . . . . . . . . . . . . . . . .   36
3.5  Assets sourced from Poly Pizza, with respective authors and attribution links.  . . . . . .   40
vii

## Glossary
AIArtificial Intelligence
CIMTConstraint-Induced Movement Therapy
DDADynamic Difficulty Adjustment
FSSFlow State Scale
HCDHuman-Centered Design
HMDHead-Mounted Display
HUDHeads-Up Display
IEETAInstitute of Electronics and Informatics
Engineering of Aveiro
MLMachine Learning
MTMirror Therapy
NPCsNon-Playable Characters
RLReinforcement Learning
SDKSoftware Development Kit
SUSSystem Usability Scale
VAR Lab  Virtual and Augmented Reality
## Research Group
VRVirtual Reality
XRExtended Reality
viii

## CHAPTER
## 1
## Introduction
Stroke is a leading cause of death and long-term disability worldwide, with over 12
million new cases expected annually and more than 94 million people currently living
with its consequences. Survivors frequently face permanent impairments affecting mobility,
cognition, speech, and vision that diminish their ability to perform daily activities and
significantly reduce their quality of life, often leading to depression and social isolation [1] [2].
While survivors may not fully return to their pre-stroke state, it is recommended that
rehabilitation begin as soon as possible to maximize the recovery of lost capacities and
improve performance in daily activities. Rehabilitation plays a central role in addressing
these challenges, particularly in restoring physical capacities such as motor function and
mobility and cognitive abilities, including attention, memory, and executive function. Despite
their proven benefits, conventional rehabilitation methods still present constraints that limit
further adherence by stroke survivors. To better understand these limitations, consider the
rehabilitation process as analogous to building a brick house, where the structure represents the
recovery of the survivor’s capacities. Just as a house cannot be built without bricks and mortar,
successful rehabilitation requires fundamental components. In this analogy, the survivor’s
spirit, motivation, determination and engagement are the bricks the essential, foundational
elements without them no progress can be made. The exercises constitute the mortar, binding
the components together and translating motivation into visible progress. However, traditional
rehabilitation often relies on the repetitive practice of the same exercises. This can make
them feel tedious, monotonous over time, deterring the patient’s motivation and engagement
the essential ’bricks’ thereby compromising the entire recovery process [2] [3] [4] [5].
Another major constraint of conventional rehabilitation programs is the fact that they
usually apply the same approach to everyone, neglecting the individual characteristics and
recovery paths of each stroke survivor. As a result, survivors who struggle with the tasks
while seeing their peers succeed may become demotivated, which again breaks the essential
## 1

’bricks’ of this process. The core issue, therefore, does not lie with the survivors themselves,
but rather with programs that fail to adapt to the specific needs and recovery pace of each
individual [2] [3] [4] [5] [6] [7] [8].
This leads to an important question, how can we create rehabilitation programs
that adapt to the individual needs of each survivor? The solution lies in introducing a
certain degree of personalization into these programs focusing on adapting to the survivor’s
specific needs and progress.
However, monitoring a survivor’s progress with the accurate precision and adapting
the exercises in real-time based on their performance is overly complicated for healthcare
professionals. Manually recording times, comparing results across sessions, and analyzing
various performance parameters is time-consuming and inclined to error. Therefore, there is a
clear need for tools that can automatically make these adjustments and assist professionals in
monitoring progress.
When considering automation, digital solutions such as computers are what comes to
mind.  A bright approach is to consider the use of Virtual Reality (
VR) combined
with Serious Games. Digital environments exploringVRare well known for creating an
immersing experience that increases user engagement. In addition,VRHeadsets can track
users hands/arms, allowing users to controlVRapplications in a natural way. This feature
can be useful for stroke survivors practicing specific movements required to recover their
lost motor skills. Serious Games are digital games which are designed for purposes beyond
entertainment, but they incorporate gamification elements similarly or equal to those found
on traditional digital games, which are responsible for keeping players motivated and engaged.
Another similarity between this type of games and traditional ones is that they also have the
benefit of improving cognitive functions. Serious Games are particularly known to be able
to adapt to user’s performance through automatic algorithms such as Dynamic Difficulty
Adjustment (DDA) ensuring that the challenge still remains in the new level of difficulty
and still maintaining the fun of playing the game. When combined these technologies have
the potential to boost user motivation and engagement. However, despite broadly positive
clinician attitudes towardVR, the vast majority of physiotherapists have yet to integrate it
into clinical practice, underscoring the urgency of developing systems that are both clinically
transparent and demonstrably usable [3] [9].
Despite the promising potential ofVRand Serious Games for stroke rehabilitation, their
adoption in clinical practice remains limited. A recent cross-sectional survey found that, while
physiotherapists generally hold positive attitudes toward immersiveVR, the vast majority
have yet to integrate it into routine care, citing barriers related to cost, usability, and lack of
clinical evidence. This gap between demonstrated potential and real-world adoption highlights
the need for systems that are not only therapeutically grounded, but also clinically transparent
and demonstrably usable. This dissertation addresses precisely this opportunity [3] [9].
## 2

1.1  Goals and Contributions
This dissertation aims to design and develop a PersonalizedVRbased Serious Game for
physical rehabilitation that trains the upper limbs and cognitive functions of stroke survivors.
The upper limb aspects that the games will focus on include arm and hand movements,
gripping strength, and motor coordination, while the cognitive functions addressed are
planning, attention, and reasoning.
To this end, the following objectives were defined:
- Develop a personalized VR serious game for stroke rehabilitation;
- Implement an automatic rule-based algorithm;
- Apply an Human-Centered Design (HCD) methodology;
- Evaluate usability, motivation, and personalization perceptibility.
To achieve this aim, the proposed serious game will need to offer the same initial
difficulty for all survivors, progressively adapting to each individual’s performance in real
time. To accomplish this, the existing literature must be analyzed to understand which
personalization and difficulty adjustment methods have already been explored and which can
be effectively integrated into the proposed concept. To accomplish the defined objectives, a
HCDmethodology was adopted, involving a multidisciplinary team that included healthcare
professionals from the Rovisco Pais Rehabilitation Center, to understand the needs,
motivations, and expectations of the target population stroke survivors and healthcare
professionals. This process informed the definition of requirements and design ideas for the
proposed system. All in all, this thesis aims to highlight the potential of personalization in
rehabilitation and to reinforce the idea thatVRand serious games can serve as valuable
complements to traditional rehabilitation programs, increasing patients’ motivation and
progress.
The contributions of this thesis are as follows:
## •
Analysis of existing literature and selection of a personalization method suited to the
stroke rehabilitation context;
•Design and development of a VR based framework incorporating two serious games for
the personalization of the virtual rehabilitation environment;
•Proposal of a Country Fair approach to stroke rehabilitation, combining immersive
environments, adaptive difficulty, and emotion recognition;
•User evaluation conducted in two distinct moments with over 38 participants, encom-
passing healthcare professionals and general users;
•Integration of an emotion recognition module as input data for the personalization
system, with shared emotional awareness displayed within the VR environment.
The results of this work have contributed to the following publications:
## •
Figueiredo, I., Palricas, D., Marques, B., Guimarães, A. R., Oliveira, S., Amorim, P., . . .
& Santos, B. S. (2026); Addressing Personalized Virtual Reality for Stroke Rehabilitation
## 3

Through a Country Fair Serious Game Approach: A Proposal; In IEEE Conference on
Virtual Reality and 3D User Interfaces Abstracts and Workshops (VRW) (pp. 914–920);
•Figueiredo, I., Marques, B., Palricas, D., Oliveira, S., Guerreiro, B., Reisinho, P., Silva,
S., Dias, S., Amorim, P., & Santos, B. S. (2026). Comparing Methods for Integrating
Personalized eXtended Reality in Stroke Rehabilitation. In EUSSET Conference on
Computer-Supported Cooperative Work (ECSCW) (pp. 1–10);
## •
B.Guerreiro, D. Palricas, I. Figueiredo, S. Oliveira, B. Marques, S. Brás, P. Amorim
and B. S. Santos (2026). "Visualization Framework for Shared Emotional Awareness in
Personalized VR for Stroke Recovery”. Proceedings of the International Conference on
Information Visualisation, IV, 1-6.
## 1.1.1  Thesis Structure
This thesis is organized into five chapters. Chapter 1 introduces the problem context,
motivation, research goals, and contributions of the present work.
Chapter 2 presents a review of the relevant literature, examining the clinical context of
stroke rehabilitation, the role ofVRand Serious Games in rehabilitation settings, personal-
ization strategies and dynamic difficulty adjustment methods, as well as the application of
HCD in the development of rehabilitation systems.
Chapter 3 describes the design and development of Country Fair VR, detailing the
conceptual framework, the two serious games developed and the personalization module.
Chapter 4 presents the two waves of user evaluation conducted, describing the methodology,
participants, procedures, and results obtained from both studies.
Chapter 5 concludes the thesis by summarizing the main findings and contributions,
discussing the limitations of the present work, and outlining directions for future research.
## 4

## CHAPTER
## 2
Background and related work
This chapter presents an analysis of key concepts associated with the themes explored in
this thesis, namely stroke and its impact on survivors, the role of Virtual Reality (VR) and
Serious Games in rehabilitation contexts, personalization strategies and adaptive systems, and
the application of Human-Centered Design in the development of rehabilitation technologies.
It is structured as a narrative review rather than a formal systematic one, with these
domains selected for their direct relevance to the research questions established in Chapter 1.
The technological core of this review, particularly the treatment of Extended Reality (XR),
personalization, and difficulty adaptation, is anchored on the scoping review conducted by
Figueiredo et al., which followed the Preferred Reporting Items for Systematic Reviews
and Meta-Analyses (PRISMA) guidelines and surveyed the SCOPUS database over the
2020–2024 period, narrowing 82 retrieved records to 39 studies through predefined inclusion
criteria and a search query spanning eXtended Reality, personalization/adaptation, rehabilita-
tion, patient, and stroke terms. The foundational clinical and theoretical concepts that lie
outside the technological scope of that review, such as stroke epidemiology, neuroplasticity,
the evidence base for established motor rehabilitation therapies, and Flow Theory, were
drawn from targeted searches of primary studies, meta-analyses, and seminal works, ensuring
that each thematic area rests on sources appropriate to its nature. [3]
2.1  Stroke causes and challenges
Stroke is defined as an acute episode of focal dysfunction of the brain, retina, or spinal cord
lasting longer than 24 hours or resulting in death, and is broadly classified into ischemic (arterial
occlusion) and hemorrhagic (intracerebral or subarachnoid bleeding) subtypes. According to
the World Stroke Organization, over 100 million people have already experienced a stroke,
more than 94 million people globally live with its consequences, and over 15 million new cases
occur annually [1] [10]
## 2
## .
## 1
Source: Living Healthy Hawaii stroke risk infographic.
## 2
World Health Organization. Stroke/Cerebrovascular accident. 2025. Available at:https://www.emro.who.
int/health-topics/stroke-cerebrovascular-accident/ [Accessed: 15 January 2026]
## 5

Figure 2.1:Overview of the principal modifiable risk factors associated with stroke, including
metabolic conditions (obesity, diabetes, and hypercholesterolaemia), behavioural factors
(physical inactivity, smoking, and heavy alcohol use).
## 1
Modifiable risk factors account for the large majority of stroke burden: 91% of stroke risk
is attributable to metabolic factors such as obesity, hyperglycemia, and hyperlipidemia, 74%
to behavioral factors including smoking, physical inactivity, and poor diet, and 29%to air
pollution. As illustrated in Figure 2.1, these risk factors span a broad range of metabolic,
behavioral, and cardiovascular domains, and the resulting impairments affect motor, cognitive,
and communication function [2] [11].
A critical but often underemphasized dimension of stroke recovery is neuroplasticity the
brain’s capacity to reorganize its structure and function in response to experience and activity.
Neuroplasticity underpins all motor rehabilitation and is maximized when practice is frequent,
task-specific, and sufficiently challenging. Recovery is not uniform over time: clinically, stroke
rehabilitation is divided into three phases with distinct biological and therapeutic profiles.
The acute phase (under 2 weeks post-stroke) prioritizes medical stabilization and early
mobilization. The subacute phase (2 weeks to 6 months) represents the window of highest
neuroplastic potential, during which intensive motor retraining yields the greatest functional
gains. The chronic phase (beyond 6 months) is characterized by slower recovery, though
meaningful improvement remains achievable through sustained practice. The system developed
in this work targets survivors in the subacute and chronic phases, where engagement and
personalization are most critical to sustaining the intensity of practice required for neuroplastic
change [6] [7].
Early initiation of rehabilitation is consistently associated with better functional outcomes,
as it capitalizes on the neuroplastic potential of the subacute phase. Among the most evidence-
supported approaches for upper limb motor recovery are Constraint-Induced Movement
Therapy (CIMT) and Mirror Therapy (MT) [2].
Through the constraint of the non-paretic limb,CIMTenforces intensive use of the
affected arm through repetitive, task-oriented practice. A systematic review and meta-analysis
## 6

by Kwakkel et al. (2015) confirmed thatCIMTproduces statistically significant improvements
in arm motor function and activity performance compared to conventional therapy, with effect
sizes that are clinically meaningful across both subacute and chronic populations. By placing
a mirror in the midsagittal plane,MTcreates a visual illusion of movement in the paretic
limb, engaging mirror neuron systems to facilitate motor relearning. A Cochrane review
by Thieme et al. (2018) analyzed 62 trials and found moderate-quality evidence that
## MT
improves motor function, activities of daily living, and pain in post-stroke patients compared
to control interventions [12] [13].
Despite their documented efficacy, both methods share a fundamental structural limitation:
they rely on the systematic repetition of a fixed exercise regime. This inherent monotony is
associated with declining motivation and adherence over the extended recovery periods typical
of stroke rehabilitation, particularly in the chronic phase where external clinical supervision
is reduced. The lack of adaptive challenge exercises that respond to the patient’s evolving
performance further compounds disengagement, as tasks that are too easy or too repetitive fail
to sustain the level of active attention required for neuroplastic change. Rehabilitation through
immersive virtual environments directly addresses both of these limitations by embedding
therapeutic motor tasks within interactive, gamified environments that adapt to the patient’s
performance, sustaining engagement across repeated sessions [3] [5] [6] [14].
2.2  Digital Realities and Serious Games in Stroke Rehabilitation
## XR
is an umbrella term encompassing technologies that alter perceived reality by inte-
grating digital elements into physical or virtual environments, includingVR, which immerses
the user in a fully digital environment, Augmented Reality (AR), which overlays digital
content onto the real world, and Mixed Reality (MR), which blends physical and virtual
elements that can interact in real time (Figure 2.2). These technologies have been considered
responses to the core limitations of traditional stroke rehabilitation: monotonous exercise
routines and declining patient motivation [15] [16] [17] [18] [19].
Figure 2.2: Reality Virtuality Continuum, adapted from [15].
A scoping review by Figueiredo et al. (2025) of 39 publications retrieved from the
SCOPUS database (2020-2024) confirms thatVRis the dominant modality, appearing in
84.6% of the analyzed studies. This dominance is attributed toVR’s capacity to generate
## 7

fully immersive, controlled environments in which patients can practice motor and cognitive
tasks without the risks of physical settings [3] [20].
Serious Games constitute the dominant intervention strategy, appearing in 67% of the
reviewed exercises. Formally defined as games designed for a primary purpose other than
pure entertainment encompassing education, training, and health serious games embed game
mechanics to engage users in the acquisition of skills or behaviors through interactive scenarios.
A prominent subtype is the exergame (a portmanteau of exercise and game): a serious game
in which bodily movement is the primary input mechanism, and the physical exertion required
to play is itself the therapeutic exercise. In stroke rehabilitation, this translates to converting
repetitive therapeutic movements into structured gameplay: from gesture-based rhythm games
targeting hand dexterity, to rule-based exergames for upper limb motor recovery, to adaptive
systems that infer affective states from movement patterns to adjust session difficulty in real
time [3] [21] [22] [23] [24] [25].
From a clinical perspective, upper limb rehabilitation is the primary target, addressed
by 66.7% of the reviewed studies a proportion that directly reflects the high prevalence
of post-stroke hemiparesis and its disproportionate impact on daily autonomy. Regarding
interaction modality, hand-tracking is used as the primary interface in 41% of studies,
outperforming physical controllers (25.6%). This shift is clinically motivated: grasping a
controller presents a significant barrier for survivors with limited hand function, whereas hand-
tracking enables natural interaction without requiring grip strength or fine motor precision, as
demonstrated by Bae and Park, whose rhythm-based serious game used gesture recognition as
the sole interaction modality and reported measurable improvements in hand rehabilitation
outcomes [3] [21].
Beyond engagement, a subset of these studies reports clinically relevant functional outcomes.
Villada Castillo et al. (2024) conducted a usability study with aVRexergame for upper limb
stroke rehabilitation and reported improvements across motor performance metrics alongside
positive usability ratings, demonstrating that serious game design and clinical efficacy are
not mutually exclusive.  Ferreira and Menezes (2020) developed one of the few systems
targeting physical and cognitive rehabilitation simultaneously within a single adaptiveVR
environment, a design choice directly relevant to the dual-domain rehabilitation goals of the
present work. Rivas et al. (2020) demonstrated that affective states including frustration
and fatigue can be inferred unobtrusively from upper limb motion duringVRrehabilitation,
providing an evidence base for emotion-aware adaptation mechanisms beyond biomechanical
metrics [22] [26] [27].
Complementing serious game approaches, 23% of the reviewed studies use simulations
of real-life therapeutic activities (e.g., mirror therapy, palmar grasping exercises), and 10%
simulate Activities of Daily Living such as shopping or household tasks, prioritizing the
recovery of functional autonomy. The addition of gamification elements to these platforms
has demonstrably reduced the monotony inherent in repetitive physical therapy [3] [5].
## 8

2.3  Personalization Strategies in VR Rehabilitation
Personalization is identified as a key element of effective therapy due to the recognition
that each stroke survivor’s course of recovery is unique and varies according to factors like
physical, cognitive, and emotional impairments. Mainstream approaches to this issue through
rehabilitation also offer standard or “one-size-fits-all” methods that do not adequately address
individual needs. The inadequacy of these processes is also established to hinder recovery due
to factors like frustration and lack of motivation on the part of survivors [3].
It is worth noting that personalization is one facet of the broader field of game adaptivity,
surveyed by Lopes and Bidarra (2011), which frames the dynamic adjustment of game
components, such as game worlds, mechanics, non-playing characters, and scenarios, as a
process steered by player modelling and realized through either offline methods, which
generate customized content ahead of play, or online methods, which adjust the experience
in real time. Framing the present work through this lens situates its real-time difficulty
adjustment within the online branch of a wider design space, while leaving the largely
unexplored offline, cross-session dimension as a direction for future work [28].
2.3.1  Taxonomy of Personalization
Current research has offered a taxonomy to structure howXRenvironments can be
adapted to individual needs. The review mentioned before (Figure 2.3) identified five key
personalization approaches, distributed as follows [3]:
## •
Behavioral Tracking (30.8%): Monitors movement, posture, and engagement pat-
terns to infer states such as frustration or fatigue, triggering adaptive responses in real
time;
## •
Rule-based Systems (25.6%): Apply predefined logical rules (e.g., “if the patient
fails three consecutive attempts, reduce difficulty”) to drive adaptation; straightforward
to implement and clinically interpretable;
•Bio-Feedback (23%): Use physiological signals such as heart rate,electromyography
(EMG), or electroencephalogram (EEG) to drive real-time adaptation based on
the patient’s measured physical or cognitive state;
•Contextual Adaptation (21%): Adjusts the session based on external variables such
as time of day, session history, or inferred patient mood, enabling session-level rather
than moment-to-moment personalization;
•Intelligent Monitoring (0%): Employs Machine Learning (ML) algorithms to
analyze longitudinal data patterns and predict user needs proactively.  Despite its
theoretical potential, no study in the reviewed corpus implemented this approach. The
scoping review attributes this gap to the substantial requirements ofMLmodels large
labeled datasets, real-time processing infrastructure, and algorithmic transparency
combined with the financial constraints of rehabilitation centers and legitimate concerns
about patient data privacy [3].
Each of these approaches presents distinct trade-offs that have direct implications for
system design. Rule-based systems are interpretable and safe to deploy clinically, but
## 9

Figure 2.3:Distribution of publications across different personalization methods,highlighting Behav-
ioral tracking as the most studied approach, followed by Rule-based and Biofeedback
methods. Adapted from [3]
their static thresholds cannot capture the non-linear and fluctuating nature of stroke recovery
a patient who fails three times due to transient fatigue is not the same as one failing due
to insufficient skill progression. Behavioral tracking offers greater responsiveness but
depends on the validity of proxies: inferred frustration from movement data is not equivalent
to measured frustration, and misclassification can produce maladaptive difficulty changes.
Bio-feedback provides the most direct physiological grounding, but its reliance on additional
sensors increases setup complexity and patient burden a significant barrier in populations
with motor and cognitive impairments. Contextual adaptation operates at a coarser
temporal resolution, making it better suited for between-session adjustments than real-time
responsiveness. Critically, none of these approaches as currently implemented maintains a
persistent model of the patient’s longitudinal trajectory the gap that Intelligent Monitoring is
designed to address, and that justifies the hybrid heuristic-clustering design adopted in the
present work (Section 2.4) [29].
2.3.2  Automation vs Clinical Control
One major issue associated with personalization is methods and styles of adaptation, and
more generally management or orchestration. While it appears that there is a preponderance
towards automatic personalization (46.2%) in existing literature, this does not necessarily
reflect professional preference [3].
A recent study show a dominant leaning toward the hybrid model (70%)(Figure 2.4).
The preference is for systems that can either suggest or execute the changes through software
but still enable the therapist to correct the system with their own interventions whenever
deemed necessary. The system designed for human involvement allows for oversight and
reduces the therapist’s burden of working manually. Additionally, concerning the point of
executing the modifications, 50% of healthcare professionals lean toward the Hybrid model
of synchronization, which aims to modify the system both during the actual process and
subsequently using historical data [4].
With respect to the data used as inputs for the personalization process, although traditional
## 10

measures of physical activity (such as range of movement) were common, personalized therapy
requirements for the survivor were seen as motivation levels (70%) and emotional state (60%)
by health professionals. This is an indication that the survivor’s psychology has been targeted
for enhanced therapy compliance [4].
Figure 2.4:Distribution of healthcare professionals votes for each personalization mode: hybrid (7),
manual (3), automatic (0).Adapted from [4]
## .
2.4  Dynamic Difficulty Adjustment (DDA) and Flow Theory
One of the central challenges of stroke rehabilitation is sustaining the survivor’s active
engagement across extended periods of repetitive exercise. A theoretically grounded response
to this challenge is found in Flow Theory, formally proposed by Csikszentmihalyi in his
seminal work Flow: The Psychology of Optimal Experience. According to this framework
(Figure 2.5), optimal experience, the Flow Zone occurs when the perceived challenge of
a task is in equilibrium with the individual’s skill level. When challenge exceeds skill, the
result is anxiety and frustration; when skill exceeds challenge, the result is boredom and
disengagement. Both conditions are detrimental to rehabilitation adherence [8] [30] [31].
The operational validity of Flow Theory in interactive contexts has been established
empirically. Jackson and Marsh developed and validated the Flow State Scale (
## FSS),
a nine-dimension psychometric instrument that measures flow across dimensions including
challenge skill balance, absorption, and sense of control. TheFSShas since been applied
to serious game contexts, confirming that well-calibrated adaptive difficulty is a significant
predictor of flow state and sustained engagement [31] [32].
While Csikszentmihalyi’s model provides the foundational account of optimal experience,
subsequent work has extended it to better fit the dual objectives of rehabilitation exergames.
The Dual Flow Model (Figure 2.6) proposed by Sinclair et al. decouples the classic at-
tractiveness axis balancing challenge and skill to sustain engagement from an orthogonal
effectiveness axis, which ensures that the physical intensity of play remains within a therapeu-
tically beneficial range. This separation is directly relevant to the present work, in which a
## 11

task must be simultaneously engaging and clinically effective: a patient may remain in flow
while exercising at an intensity that is therapeutically insufficient, a condition that single-axis
flow models cannot capture [33].
Figure 2.5:Mihaly Csikszentmihalyi’s Flow Theory: the Flow Zone is reached when challenge and
skill are in balance. Too much challenge produces anxiety, too little produces boredom.
Adapted from [30].
Figure 2.6:The Dual Flow Model for exergaming proposed by Sinclair et al. It extends Csikszentmi-
halyi’s single-axis flow with two orthogonal dimensions: an attractiveness axis, balancing
challenge against skill to sustain engagement, and an effectiveness axis, ensuring that
the physical intensity of play remains within a therapeutically beneficial range. Adapted
from [33].
2.4.1  Mechanisms of Adaptation
To operationalize Flow Theory withinVRrehabilitation, systems employDDAdefined
as the capacity of a system to modify task parameters in real time in response to the player’s
## 12

measured performance. The goal is to continuously position the patient within the Flow
Zone, preventing both anxiety from excessive difficulty and disengagement from insufficient
challenge [8].
In the literature,DDAapproaches span a spectrum of computational complexity. At one
end, heuristic methods apply fixed logical rules (e.g., “if the patient fails three consecutive
attempts, reduce difficulty by one level”) that are transparent, easy to validate clinically, and
require no training data. At the other end, Reinforcement Learning (RL) frames difficulty
adaptation as a sequential decision problem, where an agent learns a policy that maximizes
long-term engagement by exploring difficulty adjustments and observing their effect on patient
behavior. Bayesian approaches offer a middle ground, maintaining a probabilistic model
of the patient’s latent skill state that is updated after each observation, allowing principled
uncertainty quantification and personalized adaptation even with limited session data. Despite
their theoretical advantages, bothRLand Bayesian methods present significant barriers
in clinical rehabilitation contexts:RLrequires large interaction histories to converge to
stable policies, which are rarely available for stroke populations with heterogeneous recovery
trajectories, Bayesian methods require careful prior specification and are difficult to audit
or explain to clinical staff.
A further, data-driven route to personalization is the profiling of survivors through the
clustering of their performance data. Bouatrous et al., for instance, applied an unsupervised
K-means algorithm to group patients according to their measured performance during a
VRhand-rehabilitation exergame, using the resulting skill-level clusters to drive real-time
difficulty adaptation. This behaviour-based grouping is conceptually analogous to player
profiling, but is grounded in measured motor performance rather than predefined player types,
offering an interpretable middle ground between static rule-based thresholds and the opaque,
data-intensive reinforcement-learning approaches discussed above [20].
These interpretability and data requirements directly informed the design choice in the
present work, described next [3] [31] [34] [35].
2.5  Human-Centered Design and Clinical Acceptance
The effectiveness of innovative rehabilitation technologies is not contingent solely on their
technical performance, but equally on how well they integrate into clinical workflows and align
with the needs of all stakeholders. Despite growing evidence for the therapeutic potential of
VR, its adoption in clinical practice remains limited. Scoping reviews consistently identify
a persistent gap between clinicians’ generally positive attitudes towardVRand its actual
integration into routine care, attributing this gap to a complex interplay of organizational,
technological, and clinician-related barriers. This reality underscores why the latest generation
of rehabilitation systems increasingly adopts a Human-Centered Design (HCD) approach,
which engages end-users both stroke patients and healthcare professionals throughout the
entire development process [36] [37].
## 13

2.5.1  Bridging the Gap: Role of Health Care Professionals
Healthcare professionals act as gatekeepers of rehabilitation interventions: they prescribe,
monitor, and ultimately determine whether a technology is integrated into patient care.
Research consistently shows that clinician-related factors including perceived ease of use,
self-efficacy, and alignment with existing clinical protocols are among the most critical
determinants ofVRadoption in rehabilitation settings. A cross-sectional survey of French-
and Dutch-speaking physiotherapists across eight countries found that 75.5% had never
used immersiveVRin clinical practice, despite most holding positive attitudes toward the
technology and perceiving it as clinically useful. The primary barriers cited equipment cost,
lack of institutional support, and insufficient training time are organizational in nature, not
attitudinal, suggesting that acceptance alone is insufficient to drive adoption [9] [36] [37].
This disconnect between acceptance and adoption is further evidenced at the local level. A
study conducted at the Centro de Reabilitação Rovisco Pais found that existing technological
solutions frequently overlook the realities of clinical practice, failing to accommodate the
workflow and customization needs of therapists. Taken together, these findings indicate that
successfulVRrehabilitation systems must be designed not only for patients, but with and
for the clinicians who deliver care [4].
TheHCDframework also highlights a paradigm shift in what data matters most for
personalization. Traditional rehabilitation systems prioritize biomechanical metrics such as
range of motion and movement speed. However, when healthcare professionals were asked to
identify the most critical data types for tailoring recovery, they cited Motivation (70%) and
Emotional State (60%) above physical performance indicators.This finding reinforces the need
forVRsystems that go beyond motor training, integrating gamification and engagement-
monitoring features capable of detecting and responding to psychological states a design
requirement directly addressed in the present work [4].
## 2.6  Summary
The review conducted in this chapter identifies a consistent set of gaps across the clinical,
technological, and design dimensions ofVRstroke rehabilitation. Rather than recapitulating
each section independently, Table 2.1 presents these gaps in structured form, mapping each
identified limitation directly to the design decision it motivates and to the chapter where that
decision is operationalized.
These converging gaps collectively define the design space for the present work. The target
interaction modality is hand-tracking the personalization architecture is hybrid, preserving
clinical authority while automating routine difficulty adjustments, the adaptation mechanism
is rule-based and grounded in Flow Theory and the therapeutic content targets upper
limb motor function within an ecologically valid gamified environment.
## 14

Table 2.1: Summary of literature gaps and resulting design decisions.
Gap identifiedDesign decision
Traditional rehabilitation (CIMT,
## MT
) relies on monotonous repetition,
reducing adherence in the subacute
and chronic phases where sustained
practice is most needed.
Embed therapeutic motor tasks within a gamified VR
serious game (Country Fair VR) to sustain engagement
across repeated sessions.
Physical controllers exclude stroke
survivors with severe upper limb
impairment, hand-tracking dominates
in 41% of the reviewed literature but
remains underexplored for
archery-based interaction.
Adopt hand-tracking as the sole interaction modality,
removing the grip requirement and enabling natural
gesture-based gameplay.
Existing VR rehabilitation systems
adopt fully automated personalization,
which conflicts with the clinical
preference for therapist oversight
reported by 70% of healthcare
professionals.
Implement a hybrid personalization model: rule-based
DDA handles real-time difficulty adjustment; the
therapist retains override authority via a dedicated
interface.
DDA approaches in the literature
prioritize biomechanical metrics;
neither engagement nor emotional
state is used as an adaptation signal,
despite clinicians rating motivation
(70%) and emotional state (60%) as
the most critical personalization
inputs.
Integrate gamification mechanics reward systems,
progressive challenges, environmental feedback designed to
maintain the patient within the Flow Zone as defined by
## Csikszentmihalyi [30].
Advanced ML-based DDA
approaches (RL, Bayesian
methods) require large datasets and
produce non-interpretable policies,
making them unsuitable for clinical
deployment with heterogeneous stroke
populations.
Adopt a rule-based adaptation mechanism: performance
thresholds trigger discrete difficulty changes, ensuring
clinical transparency and immediate auditability by the
therapist.
Only 30.8% of reviewed systems
provide access to previous session
data; longitudinal progress tracking
essential for adaptive rehabilitation
remains largely absent.
Implement a session history module that logs
performance metrics across sessions, enabling both
therapist review and data-informed difficulty seeding at
session start.
## 15

## CHAPTER
## 3
Personalized VR-based Country Fair
This chapter presents the design and implementation of Country Fair VR, a Virtual
Reality (VR) serious game prototype developed for stroke rehabilitation. Following the
Human-Centered Design (HCD) methodology, it covers the system architecture, the
Country Fair environment and its integration with the personalization model, the therapeutic
justification and interaction mechanics of each mini-game, the CarnyWise personalization
module, and the emotion recognition module incorporated as a complementary increment to
the system.
## 3.1  Methodology
The development of Country Fair VR followed aHCDapproach, consistent with the
methodology adopted by the research group in which this work is integrated, as illustrated
in Figure??. The process began with visits to the Rovisco Pais Rehabilitation Center
(Figure 3.2), which provided direct insight into the clinical context and the needs of stroke
survivors and healthcare professionals. These findings, combined with the collective knowledge
of the research group, informed the definition of the Country Fair concept and the selection
of hand-tracking as the primary interaction modality a choice grounded in the group’s
experience that controller-free interaction is better suited to the motor constraints of the
target population. The primary objective driving all design decisions is the personalization
of the rehabilitation experience during the execution of the activities, adapting
in real time to each survivor’s individual performance and skill level.  This focus on in-
session personalization distinguishes this work from approaches that rely solely on pre-session
configuration by healthcare professionals. The concept was subsequently refined through
iterative development and two waves of user testing, as detailed in Chapter 4 [4] [5].
## 16

Figure 3.1: HCDmethodology adopted for the design and development of the personalizedVR
serious game, structured into iterative phases spanning requirements extraction with
stroke survivors and healthcare professionals, prototyping, and waves of user testing.
Figure 3.2:Illustration of the research team conducting a user study, with a stroke survivor answering
a post-task questionnaire, with the support of the research members.
## 3.2  Architecture
The system architecture of Country Fair VR, represented in Figure 3.3, was grounded
in the conceptual framework proposed (Figure 3.4), which defines a closed-loop rehabilitation
model integrating a VR environment, a Data Hub, a Personalization Engine, and
a Monitoring Tool. The implemented architecture operationalizes this model through
three concrete modules: a Game module, a Personalization module, and an Emotion
Recognition module. The Data Hub responsible for aggregating session data and com-
paring current performance against historical records to inform adaptation decisions and the
## 17

Monitoring Tool a dedicated interface for healthcare professionals to supervise and adjust
the rehabilitation process are part of the long-term vision for the framework but were not
implemented within the scope of this dissertation [5].
Figure 3.3:System architecture of Country Fair VR. The architecture contains three primary
modules: (1) a Personalization module, managed by the Carny Wise character, which
signals the Mini-Game Manager to dynamically adjust adaptive parameters and deliver
multimodal contextual feedback; (2) a Game module, structured around a Country
Fair Environment as the central hub, where the player can select between two distinct
mini-game environments, Archery and Frisbee, whose configuration can be modified by
the Personalization module through the adjustment of adaptive parameters. A Healthcare
Professional interacts with the system to configure and monitor patient sessions, while
the Stroke Survivor engages with theVRenvironment through a head-mounted display.
The game module is the main one of the system, which presents the initial scene, a vibrant
“Country Fair” environment within the survivor’s Head-Mounted Display (HMD).
Here, players receive a brief introduction outlining what they will do in the game. Once the
introduction is finished, they can select one of the available mini-games: the Frisbee or the
Archery as illustrated in Figure 3.5
During a survivor’s first playthrough, both mini-games provide a short explanation of the
rules alongside a practical tutorial. Within these mini-games, survivors must achieve a specific
target, simulating a clinical session goal. Upon reaching this goal, they return to the country
fair, where a congratulatory message is displayed to positively reinforce their performance.
Throughout the gameplay, the mini-games’ difficulty dynamically changes based on the user’s
performance, controlled by the Personalization Module. Additionally, players can return
## 18

to the country fair at any time by simply pressing a button located on their right side.
Figure 3.4:Conceptual framework for a closed-loopVRrehabilitation system combining Dynamic
Difficulty Adjustment (DDA) and hybrid personalization to sustain motivation and
promote neuroplasticity in stroke survivors. Adapted from [5].
Healthcare professionals can manipulate this module using a peripheral keyboard connected
to theHMD. To support both development testing and clinical oversight, a set of keyboard
shortcuts referred to as cheatcodes were implemented, allowing runtime manipulation of
the game state without interrupting the session. Table 3.1 lists all available commands, their
function, and the context in which they apply.
A detailed discussion regarding how the survivor interacts with the game, including the
specific rules, goals, and mechanics of the mini-games, will be provided in Section 3.4.1.
The personalization module represents the main contribution of this dissertation and
will be discussed in detail in Section 3.3.  The emotion recognition module appears
highlighted in Figure 3.3 not because it was a primary objective of this dissertation, but rather
because it is a complementary integration of a colleague’s work (Bianca Guerreiro) to explore
the potential of emotional awareness within the framework, as detailed in Section 3.5 [38].
## 19

Figure 3.5:The two mini-games integrated in the Country Fair VR system: the Archery mini-game
(left), where the player uses a virtual bow to pop balloons, and the Frisbee mini-game
(right), where the player throws a frisbee towards target areas for a virtual dog to
retrieve.
## 1
VR Personalized Country Fair Demo: Click Where To Watch.
## 20

Table 3.1: Keyboard cheatcodes available to healthcare professionals during runtime.
InputFunctionContextPrecondition
introSkips the fair introduc-
tion sequence
Fair SceneIntro not completed
archeryLoads  the  Archery
mini-game
Fair SceneIntro completed
frisbeeLoads the Frisbee mini-
game
Fair SceneIntro completed
returnReturns to the Fair
## Scene
Both mini-gamesNone
tutorialSkips the mini-game
tutorial sequence
Both mini-gamesTutorial not completed
increaseSignals CarnyWise to
raise the difficulty by
one level
Both mini-gamesTutorial completed
decreaseSignals CarnyWise to
lower the difficulty by
one level
Both mini-gamesTutorial completed
resetResets the difficulty to
the first level
Both mini-gamesTutorial completed
e(expression_name)Displays the specified
emotion via the emoji
representation
Both mini-gamesTutorial completed
positive/ negativeShifts  the  emotion
slider right or shifts
it  left,  respectively,
triggering  the  slider
representation
Both mini-gamesTutorial completed
f(expression_name)Displays the specified
emotion via the facial
expression representa-
tion
Both mini-gamesTutorial completed
scoreSimulates a successful
player score
Both mini-gamesTutorial completed
missSimulates  a  player
miss
Both mini-gamesTutorial completed
dogSimulates a score by
landing the frisbee in
the dog’s surrounding
area
Frisbee onlyTutorial completed
## 21

3.3  Country Fair Model and Personalization Integration
The game is set in a “Country Fair”, an environment chosen for its cultural resonance,
emotional value, and ability to promote a positive atmosphere. The fair is structured around
a set of tents, each representing a distinct rehabilitation activity in the form of a serious
game. To select a mini-game, the player looks directly at the corresponding tent, triggering
a contextual prop representing the respective mini-game alongside a spatial 2D element
displaying the mini-game’s title and an interaction button, which when pressed transports
them to the selected activity. This tent based structure was conceived as a scalable system:
while the scope of this dissertation covers two mini-games developed by the author, the
model is designed to accommodate additional games in the future, including those already
developed by other members of the research group, progressively expanding the range of
rehabilitation activities available to survivors. The visual environment is designed to be
vibrant and stimulating, incorporating dynamic structures, such as a rotating Ferris wheel,
and ambient animals. Furthermore, to mitigate the feeling of social isolation often associated
with rehabilitation therapy, the fair is populated with a crowd system of Non-Playable
Characters (NPCs), providing a crucial sense of social presence [4] [39].
Within this dissertation, two mini-games were developed and integrated into the system:
an Archery game, where the player uses a virtual bow to pop balloons, and a Frisbee
game, where the player throws a frisbee towards target areas for a virtual dog to retrieve, as
illustrated in Figure 3.5.
To reinforce the carnival aesthetic and enhance visual engagement, a post-processing effect
was applied to the virtual environment, elevating contrast and saturation levels. This stylistic
choice contributes to a more immersive and emotionally resonant experience. Complementing
the visual treatment, the Heads-Up Display (HUD) and the dialogue box ofNPCsemploys
a custom carnival-style font (Carnevalee FreakShow), ensuring typographic consistency
with the thematic setting and improving readability for users.
Upon entering the fair, the player receives a brief introduction given by the owner of the
fair (Zeca Bigodes) (Figure 3.6). He explains the purpose of the game to the player, lists
the mini-games that the fair offers, and introduces another character, Carny Wise (Figure
3.7), who will explain the rules of each mini-game, provide a practical tutorial for first-time
players (including a demonstrative video showing the gesture to be performed), and ensure
that the game matches the player’s skill level serving as the in-game representation of the
personalization module. The name CarnyWise derives from the colloquial North American
word “Carny”, referring to carnival workers, and “Wise”, indicating an intelligent character.
As illustrated in Figure 3.3, this character controls a closed-loop rule based personalization
system that continuously evaluates player performance to maintain a “Flow State” an
optimal balance between skill and challenge that prevents both anxiety and boredom [8] [31].
The current mini-game available are: an Archery game, where the targets are balloons,
and a Frisbee game, where the player must throw a frisbee to certain areas for a dog to catch
in order to score points.
## 22

After the player finishes playing one of these games, they are transported back to the fair,
and the owner congratulates them on his performance as shown in Figure 3.6.
Figure 3.6:Zeca Bigodes dialogue panels: introduction to the fair (top), positive performance
feedback after completing the Frisbee mini-game (bottom left), and positive performance
feedback after completing the Archery mini-game (bottom right).
## 23

Figure 3.7:Carny Wise tutorial sequence: instructional dialogue accompanied by a demonstrative
video showing the gesture to be performed, during the Frisbee mini-game tutorial (left)
and the Archery mini-game tutorial (right).
## 3.3.1  Personalization Hybrid Approach
According to the preferences expressed by healthcare professionals, the personalization
module was designed as a hybrid personalization model. This architecture combines
real-time algorithmic adjustments with the capability for human intervention, effectively
dividing the system into a manual and an autonomous component [4].
## Manual Component
Although initially implemented as developer shortcuts during the development phase,
these cheatcodes (see Table 3.1) remain active in the final prototype build for theHMD.
This functionality empowers healthcare professionals supervising the rehabilitation sessions to
manually intervene and adjust the mini-games’ difficulty based on their clinical judgment,
provided a keyboard is connected to the HMD.
## Autonomous Component
Beyond this manual override capability, the core of CarnyWise operates through an
autonomous algorithm logic described in Figure 3.8 divided into two main branches:
•Failure Handling: If the player misses more than four consecutive times, the system
immediately signals the mini-game manager to decrease the difficulty. This mechanism
## 24

was designed to automatically adjust the difficulty whenever the current level proves
unsuitable for the player, including situations of potential fatigue, thereby preventing
frustration. It should be noted, however, that the number of consecutive misses con-
stitutes only an indirect and approximate indicator of fatigue, rather than a concrete
measure of it. A more rigorous assessment of this phenomenon would require other
methods, such as behavioral detection or biofeedback techniques , which are capable
of providing more direct physiological or behavioral indicators of the player’s fatigue
state [3];
•Precision Analysis: When the player scores, the system calculates the “Score Precision”
percentage. This metric is calculated as the inverse of the number of attempts required to
achieve a successful score (i.e.,
## P=
## 1
n
, wherenrepresents the total number of attempts).
This branch relies on a heuristic, rule-based behavior system, which the literature
identifies as the most common for stroke rehabilitation. Specifically, a high precision
## (
≥60%) increments an Excel Counter, while a low precision (≤40%) increments
a Struggle Counter.  Reaching a predefined threshold in either counter triggers an
automatic adjustment in the current mini-game’s difficulty [3].
It should be noted that the specific numeric values commanding these branches, both
the 60%/40% precision bounds and the thresholds that trigger an adjustment, were set to
provisional values during development for testing purposes, rather than being derived from a
systematic empirical or clinical calibration. They were chosen pragmatically to validate the
adaptation logic and are exposed as configurable parameters, so that the mechanism itself is
decoupled from any particular tuning.
## 25

Figure 3.8:Flowchart of the CarnyWiseDDAalgorithm, illustrating the two main adaptation
branches: the Failure Handling branch, which triggers a difficulty decrease upon four or
more consecutive misses, and the Precision Analysis branch, which accumulates Excel
and Struggle counters based on score precision thresholds (≥60% and≤40%, respectively)
to drive automatic difficulty adjustment.
## Multimodal Difficulty Feedback
Providing immediate feedback is a crucial precondition for keeping players immersed in
a state of flow. To reinforce this personalization and effectively communicate system state
changes, the game utilizes a context-aware dialogue system driven by external JSON files.
This system delivers dynamic and randomized motivational text feedback based on the user’s
performance.  Rather than explicitly stating that the difficulty has changed, the system
provides subtle hints using common Portuguese colloquial expressions. For example, phrases
such as “Isso foi canja. Vamos passar às coisas sérias” or “Vamos aliviar um bocado. O
## 26

importante é não parar”, are used to indicate an increase or decrease of the challenge level. It
is worth noting that this feedback mechanism was not formally validated in terms of its direct
impact on participants’ motivation levels. Rather, its purpose was to give players a clear
sense that the difficulty was being adjusted, preventing them from unknowingly continuing to
play based on an outdated difficulty level and, consequently, feeling frustrated by a mismatch
between their performance and the difficulty they believed they were facing [8].
Furthermore, to complement the text displayed in a fixed location anchored to the player’s
field of view, the system incorporates an auditory cue that is triggered whenever the difficulty
level is adjusted. This sound provides players with immediate auditory feedback in addition
to the visual indication, reinforcing their awareness that a change in difficulty has occurred,
even if their attention is not directly focused on the displayed text at that moment.
## Adaptation Mechanism
While CarnyWise dictates the timing of the adaptation, the specific adjustments are
executed by the individual mini-game managers. To provide the infinite progression essential
for long-term rehabilitation, the adaptation parameters are divided into two distinct categories
as shown in Table 3.2:
•Physical/Quantitative Parameters: Parameters such as target distance and spawn
quantity scale according to a code-based linear progression model (y=mx+b). This
model provides practically unbounded difficulty scaling, subject to physical caps applied
where clinically or spatially appropriate (e.g., the dog’s distance is capped at 14 m
and balloon density is bounded by the spawner area), ensuring the challenge remains
progressive without becoming physically unreachable;
•Cognitive/Complexity Parameters: Parameters such as the probability of a target
moving are managed using Unity’s Animation Curve feature (Figure 3.9). Despite
the name, these curves are highly versatile and can be repurposed as difficulty curves,
providing a smoother experience by continuously sampling difficulty values as the player
progresses and supporting practically infinite progression, which is particularly suitable
for long-term rehabilitation scenarios. Unlike the previous parameters, which rely on
explicit mathematical formulas, the Animation Curve is defined in the Unity Inspector
and used at runtime as a static, continuous difficulty function, conceptually similar
to difficulty curves described in the literature. This visual authoring process allows
developers and clinicians to specify non-linear difficulty progressions without requiring
advanced mathematical or programming knowledge. Each parameter is governed by its
own dedicated curve, allowing independent fine-tuning of its progression rate [40] [41].
## 27

Table 3.2: Adaptation Parameters per Minigame
Parameter TypeFrisbee GameArchery Game
Physical/Quantitative
## Parameters
## Target Quantity:  Infinite
Scaling: Increases by roughly
1 target every 5 levels.
Distance: Linearly increases
the dog’s distance (capped at
## 14m).
## Target Quantity:  Infinite
Scaling: Increases by roughly
1 balloon every 2 levels.
Density: Increases the num-
ber of balloons that the balloon
spawner can spawn.
Cognitive/Complexity
## Parameters
Movement: Percentage of tar-
gets moving laterally.
Visibility: Percentage of tar-
gets blinking/disappearing.
Movement: Percentage of bal-
loons moving.
Transparency: Percentage of
balloons becoming translucent
## (ghosts).
Color Rule: The specific bal-
loon color required to score
changes dynamically with dif-
ficulty, preventing repetition.
The HUD renders the required
color rule text in the corre-
sponding color to reinforce the
visual association.
Figure 3.9:Example of two Unity Animation Curves used in the Archery Mini Game: the left one
is used to configure the balloons that move, and the right one to configure the balloons
whose transparency changes.
## 28

3.4  Mini-Games
The selection of the mini-games for the “Country Fair” was driven by a literature
review on the therapeutic benefits of each activity, aimed at creating a comprehensive
rehabilitation protocol. Rather than offering random activities, these specific mechanics were
chosen to provide a complementary balance between static fine motor control and dynamic
gross motor coordination, grounded in recent literature on adapted physical activities and
specific requirements gathered from healthcare professionals. The two mini-games presented
in this dissertation are part of a broader set of activities proposed, developed and refined in
collaboration with healthcare professionals, whose clinical input was fundamental in defining,
discussing, and improving the proposed concepts [39].
This section details how each mini-game serves as a complementary exercise for both
upper limb motor function and cognitive rehabilitation, while also addressing other specific
needs identified by healthcare professionals. It further describes the interaction mechanics
and gestures required to play each game.
## 3.4.1  Common Interactions
Throughout the game, button-based interactions are performed using a poke gesture, in
which the player extends their index finger as shown in Figure 3.10 to physically press a virtual
button. This gesture was chosen due to its widespread adoption inVRapplications as a
natural and intuitive interaction for pressing buttons, and because it is natively supported by
the Meta Interaction Software Development Kit (SDK) through its direct interaction
system, reducing implementation complexity while ensuring a familiar user experience. This
gesture is consistently used across all parts of the game, both for advancing dialogues and
for navigating between areas, such as returning to the fair from a mini-game or selecting a
mini-game from the fair.
Figure 3.10:Interaction gestures used to interact with the game’s virtual environment: Poke gesture
(left) Grab gesture (right). Created by Gemini Pro.
## 29

3.4.2  Archery Mini-Game
## Upper Limb Motor Functions Justification
The act of drawing and holding a bow constitutes a highly effective bimanual static
exercise that targets specific muscle groups differently in each arm. Recent electromyographic
analyses demonstrate that archery heavily activates the biceps brachii and latissimus dorsi
in the drawing arm, while the non-dominant bow arm relies significantly on the triceps
brachii for elbow extension and the lateral deltoid for shoulder abduction. By replicating
this precise muscular tension in
VRthrough the “grab” gesture, the prototype aims to
stimulate this complex neuromuscular engagement. The clinical benefits of this engagement
are well-documented: research by Mahlovanyy et al. (2023) on adapted archery for Paralympic
athletes demonstrated that specific archery training led to a reliable increase in handgrip
force (rising from 37.4 kg to 45.5 kg) and improvements in the muscular endurance of the
shoulder girdle. Furthermore, archery interventions have been proven to significantly improve
hand flexibility, finger dexterity, and overall upper extremity motor functions in patients with
neurological disorders, making it a highly suitable complementary physiotherapy for motor
recovery [42] [43] [44] [45].
## Cognitive Justification
Beyond the physical aspect, the archery mechanic inherently supports cognitive and
psychological recovery. Successful archery requires a high capacity for decision-making and
“afferent synthesis” within micro-intervals of time, contributing significantly to the athlete’s
psycho-emotional stability. In fact, participating in archery exercises has been shown
to alleviate depressive symptoms and improve overall psychological well-being by fostering
a sense of self-fulfillment through hitting targets. Furthermore, recent neurorehabilitation
research underscores a strong physical-cognitive interplay, noting that improvements in physical
markers such as the handgrip strength developed during archery are positively correlated with
enhanced cognitive performance, including memory and processing speed [43] [44] [45].
To further amplify this cognitive training, our prototype implements a dual-task paradigm
by requiring users to pop only balloons of a specific color. This design choice is supported
by the work of Bouatrous et al. (2023), who utilized color-matching tasks (placing fruits in
corresponding baskets) to measure and train selective attention. Consequently, theVR
Archery game serves as a composite exercise: the act of aiming trains emotional stability and
decision-making, the physical grip strengthens critical neuromuscular pathways, and the color
rule trains inhibition and selective attention [20].
## Interaction Mechanics
In this mini-game, the bow is currently attached to the player’s left arm. The player uses
their right hand to perform a grab gesture (Figure 3.10) to pull the bow’s string, as ilustrated
in Figure 3.11. The more they pull the string, the stronger the arrow’s launch force will be,
although the bow can only be pulled up to a certain length. To launch the arrow, the player
must open all fingers of their hand, releasing the string. An issue was observed regarding
## 30

hand-tracking during this gesture. Because the drawing motion pulls the string backward,
when the hand moved behind the headset’s tracking sensors the string-pull was occasionally
not detected, as the hand left the cameras’ field of view. During this mini-game’s tutorial,
performing the gesture behind the sensors could, in some cases, lead to a crash of the game.
Figure 3.11:A participant performing the Archery mini-game gesture while seated, using the Meta
Quest 3 headset with hand-tracking interaction.
Figure 3.12 shows the forces acting on the arrow’s movement (launch, lift, gravity, and
drag). These forces cause the arrow to follow a parabolic trajectory. When the arrow collides
with any object, it automatically returns to the player’s bow, and the player must pull
the string again to launch it. As mentioned before, to score points the player must pop
a specific balloon color, which is displayed in the mini-game’sHUDalongside their score,
streak, session goal, and the emotion representation provided by the Emotion Recognition
Module, anchored to the player’s field of view to remain consistently visible regardless of
head orientation. Additionally, there is an audience that surrounds the player and when they
score, the audience performs a jumping animation followed by a spin in the air, providing
positive visual feedback to encourage and keep the player motivated. Table 3.3a shows how
the score varies depending on the balloon’s parameters.
## 31

Figure 3.12:Schematic representation of the forces acting on the mini-game props during their
motion: on the left, the arrow (archery mini-game) subject to launch force, gravity, and
drag; on the right, the frisbee (frisbee mini-game) subject to launch force, gravity, drag,
and lift. Created by Gemini Pro.
## Table 3.3: Mini Games Score System
## (a) Archery Mini Game
Balloons ParametersScore Points
## Static1
## Moves2
## Transparency Can Change3
## Moves And Transparency Can Change6
## (b) Frisbee Mini Game
Dog AreaScore Points
## --8
Other Score Areas ParametersScore Points
## Static1
## Moves2
Can blink/Disappear3
Moves And Can blink/Disappear6
3.4.3  Frisbee Mini-Game
## Upper Limb Motor Functions Justification
Although the user remains seated, the throwing motion requires a dynamic extension of
the elbow and a coordinated flexion and extension of the wrist. This is clinically relevant
because it combats the flexor synergy pattern (the tendency for the paretic arm to remain
bent against the body), encouraging the user to extend the limb outwards.
Supported by the findings of Kao and Wang (2018), participating in Frisbee-based exercises
requires significant upper limb engagement, specifically improving the functional abilities
## 32

of reaching, holding, and grasping. Furthermore, this specific training effectively enhances
upper limb powerwhich is defined as the product of muscle strength and speed as well as
gross and fine hand motor dexterity. In the virtual environment, the survivor must not only
hold the disc but release it at the precise moment of the swing to achieve the desired trajectory.
This mechanic explicitly trains the timing of motor execution, directly rehabilitating these
crucial upper extremity functions [46] [47].
## Cognitive Functions Justification
Beyond motor skills, this mini-game addresses specific cognitive domains. Research by
Kao and Wang (2018) highlights that Frisbee activities significantly improve hand-eye
coordination, direction control coordination, and reaction capability. Moreover, a study
by Koutsobina et al. (2021) emphasizes a strong correlation between perceptual-motor
development (such as visual-motor integration) and the overall level of cognitive and mental
functioning [46] [47].
In my prototype, the survivor must continuously calculate the distance to the dog or
the secondary target zones, adjusting the force and angle of the throw accordingly. This
process requires the brain to integrate visuospatial information with motor output in real-time.
Furthermore, the requirement to react to the dog’s position and to the toggling visibility of the
target zones trains processing speed and visual attention. This design effectively transforms
the physical throwing gesture into a robust neurocognitive exercise that fosters both motor
organization and cognitive development [46] [47].
The Role of the Dog
The inclusion of a virtual dog as the receiver is a design response to the specific needs
identified in our survey with healthcare professionals [4].
The results indicated two critical requirements:
•Guidance when Lost: Professionals reported that 60% of survivors often feel confused
or lost inVR. To mitigate this, the dog acts as an active navigational cue. By barking
or looking towards the target area, the dog directs the user’s attention, preventing
disorientation without the need for intrusive text instructions [48] [49];
## •
Social Presence: The survey also emphasized the need to combat social isolation.
Professionals suggested the use of ‘‘animal-shaped
NPCs’’ to provide emotional
support. The dog serves as a social agent, reducing the feeling of loneliness in the virtual
world and creating an emotional bond that encourages the survivor to continue the
therapy [4].
## Interaction Mechanics
Upon entering the mini-game, the dog starts walking and chooses a random position in
front of the player. The frisbee is positioned in front of the player’s head and follows its
movement. This allows the player to grab the frisbee with their main hand by performing the
gesture shown in Figure 3.10.
## 33

To throw the frisbee, the player must extend their arm and open all fingers of the hand
they used to grab it, as illustrated in Figure 3.13. The quicker they perform these actions, the
more intense the launch force will be, resulting in greater velocity. However, to make a more
accurate throw, before extending their arm the player must bring the hand that grabbed the
frisbee nearly to their opposite waist.In contrast to the archery gesture, no detection issues
were observed when the hand was brought near the hip prior to the throw: the pinch pose
was correctly identified in this position, and hand-tracking remained reliable throughout the
throwing motion.
Figure 3.13:A participant performing the Frisbee mini-game throwing gesture while seated, using
the Meta Quest 3 headset with hand-tracking interaction.
Figure 3.12 shows the forces acting on the frisbee’s movement. These forces cause the
frisbee to have a more linear trajectory at the beginning a result of the frisbee’s lift force and
then follow a more parabolic trajectory as it nears the ground.
As mentioned before, for the player to score points, the frisbee must land in an orange
area surrounding the dog or in other orange areas. Table 3.3b shows the score values for each
area and their parameters, which will be discussed in subsection 3.3.1. The player’s score,
streak, session goal, and the emotion representation provided by the Emotion Recognition
Module are displayed on the sameHUDas the Archery mini-game, without the color
rule indicator, anchored to the player’s field of view to remain consistently visible regardless
of head orientation.
After the frisbee lands in one of these areas, all areas disappear, a sound score effect is
played, and balloons appear, rise, and pop to encourage the player and keep them motivated.
Following that, the dog jumps giving feedback to the player that it noticed the frisbee has
landed and goes to its position to catch it. After catching it, the frisbee appears in the dog’s
mouth, and delivers it to the player.
Once delivered, the dog chooses a random position in front of the player, and new target
areas appear for the player to continue the game. However, if the frisbee does not land in one
## 34

of these scoring areas, the frisbee automatically reappears in front of the player.
## 3.5  Emotion Recognition Module
This module was not a primary objective of my dissertation, rather, it served as a
complementary integration to verify whether the practical component of a colleague’s master’s
dissertation (from the same research group) could be successfully merged with my prototype
to create a more robust future application. My colleague’s (Bianca Guerreiro) research, titled
“Exploring Emotion Recognition for Stroke Rehabilitation: Towards Personalized Virtual Reality
Serious Game aims to understand how emotion recognition can be utilized in personalized
VRenvironments for post-stroke rehabilitation, exploring how detecting a user’s emotional
state can foster a more adaptive and engaging therapeutic experience [38].
This collaboration had two main benefits. First, it served as a proof of concept that shared
emotional awareness can be relayed into the rehabilitation environment, extending the system
beyond motor and cognitive training toward the mitigation of social isolation. Second, and
from an architectural standpoint, integrating an externally developed module validated the
modularity of Country Fair VR: by consuming emotional state as a representation-agnostic
output, the system accommodated a colleague’s independent research without modifications
to its core, demonstrating extensibility toward a more robust future application.
The proposed architecture, illustrated in Figure 3.3, describes a scenario in which a
stroke survivor’s emotional state is captured, transmitted to a remote server, and relayed to
another survivor’s mini-game environment. Depending on the available specificity, the system
communicates this state through one of three representation formats, selected randomly
when entering a mini-game and adjustable via cheat codes(see Table 3.1): an emoji-based
representation, which displays the recognized emotion as a simplified symbolic cue widely used
as a substitute for emotional expression and shown to support rapid, intuitive interpretation; a
slider-based representation, which reflects the emotion category (positive, neutral, or negative)
as a position along a continuous positive-versus-negative scale initialized at a neutral value
positive emotions shift the indicator toward the right, negative emotions shift it toward the left,
and neutral emotions leave it unchanged as illustrated in Figure 3.14 and a facial-expression-
based representation, which displays a reference image corresponding to the detected facial
expression within the game environment, offering a more concrete and recognizable emotional
cue. Table 3.4 lists all supported emotion types alongside their corresponding emojis, facial
representations, and categories [38] [50] [51] [52].
Although the original concept envisioned capturing emotions directly through theHMD’s
internal sensors, hardware constraints, specifically, the absence of a headset equipped with
internal facial-tracking cameras, made this unfeasible. To replicate and validate the intended
behavior, a laptop webcam was used instead to capture participants’ facial expressions and
infer their emotional states, as shown in Figure 3.15.
## 35

Figure 3.14:Evolution of the emotion slider across three representative states: negative (left),
neutral (center), and positive (right). Positive emotions detected shift the slider upward,
negative emotions shift it downward, and neutral emotions leave it unchanged. The
slider is initialized at a neutral value.
## Table 3.4:
All supported emotion types with their corresponding emojis, facial expression representa-
tions, and valence categories (Positive, Negative, or Neutral).
EmotionEmojiFaceCategory
HappyPositive
SadNegative
AngryNegative
FearNegative
SurprisePositive
DisgustNegative
NeutralNeutral
## 36

Figure 3.15:Illustration of the emotion recognition module: a laptop webcam captures the user’s
facial expressions (bottom-left), which are classified into one of the supported emotion
categories (top-left), and the resulting emotion is relayed to the stroke survivor’s VR
mini-game environment as an emoji overlay, exemplified here by a happy emotion in
the Frisbee mini-game and a neutral emotion in the Archery mini-game (right).
## 37

3.6  Technologies and Implementation
This section details the technological foundation of Country FairVR, describing the
hardware platform on which the prototype runs, the software tools and engine used to build
it, and the visual art direction that guided the production of its assets.
## 3.6.1  Hardware
At the hardware level, the prototype utilizes the Meta Quest 3 Extended Reality (XR)
headset.This headset can recognize and interpret hand movements and gestures without the
need for physical controllers, an advantage for individuals with neurological limitations or
mobility issues affecting fine motor control. Furthermore, its robust tracking capabilities
support the assessment and improvement of gross motor functions making it highly suitable
for accurately detecting the wide-amplitude gestures required by the selected mini-games [53].
The hand-tracking latency on the Quest 3 is reported at around 70 ms, which the
manufacturer classifies as real-time tracking
## 2
. In practice, however, this classification does
not guarantee an imperceptible delay: several users still report noticing a slight lag between
physical movements and their reflection in the virtual environment. This residual latency,
combined with the passthrough feed itself, can still contribute to cyber-sickness, meaning that
low latency alone does not fully eliminate the risk of discomfort in XR applications.
Additionally, the hardware setup leverages 3D spatial audio to naturally guide the players
through the virtual environment. While the use of external headphones is highly recommended
to achieve maximum immersion, it is not mandatory, as the Meta Quest 3 headset already
features built-in speakers with robust spatial audio quality. Finally, the hardware configuration
supports the connection of a peripheral keyboard (e.g., via Bluetooth) directly to theHMD.
This addition is particularly useful for healthcare professionals, as it allows them to use
cheatcodes (see Table 3.1) to manually manipulate the game state during runtime, such as
adjusting the mini-game difficulty to suit a patient’s needs
## 3
## .
## 3.6.2  Software
The software was developed using the Unity game engine, which was chosen to facilitate
and accelerate the development process through the Meta XR Interaction SDK a free,
modular library for Unity and Unreal Engine designed to help developers create high-quality,
intuitive, and natural hand and controller interactions inVR. C# was used for scripting the
game logic, physics simulations, and the DDA algorithm.
Regarding the visual assets, while some 3D models and animations were custom-built using
Blender, more detail about the 3D models will be discussed in Section 3.6.3. Furthermore,
FMOD Studio was integrated to author and manage the project’s audio. It was used both
as the 3D spatializer for the spatial audio cues and to build multi-instrument/randomized
## 2
Matts  Digital.   Is Hand Tracking on the Meta Quest 3 Accurate and Responsive?    2026.
Available   at:https://knowledge.matts-digital.com/en/virtual-reality/meta/meta-quest-3/
is-hand-tracking-on-the-meta-quest-3-accurate-and-responsive/ [Accessed: 27 March 2026].
## 3
Steighner, M. Meta Quest 3 Review I’m Finally Not Disappointed. COGconnected, 2023. Available at:
https://cogconnected.com/review/meta-quest-3-review/ [Accessed: 27 March 2026].
## 38

containers that vary sound pitch and volume at playback, preventing auditory repetition and
enhancing the overall immersive experience.
3.6.3  Visual Art Direction and References
To establish a coherent visual identity for the “Country Fair” environment,an art
direction process was conducted prior to asset production. Reference imagery was gathered
using the keywords Carnival and Country Fair on Pinterest, with the goal of identifying a
suitable color palette, environmental props, and an overall aesthetic register consistent with
the thematic setting. The collected references, illustrated in Figure 3.16, informed several
key design decisions: the environment should adopt a vibrant, warm color palette, iconic
fairground elements such as a Ferris wheel, red-and-white striped carnival booths, popcorn
carts, balloons, and marquee tents should be present to reinforce environmental legibility and
cultural familiarity.
Figure 3.16:Art direction reference board for the “Country Fair” environment. Environmental
references (carnival tents, Ferris wheel, fairground props, and color palette) were
gathered from Pinterest using the keywords Carnival and Country Fair. Additionally,
the cover art and host character from the Carnival Games series (2K Play, Nintendo)
informed the stylized, cartoon-like design of the game’sNPCs, while the disc dog
activity from Wii Sports Resort (Nintendo, 2009) served as the primary gameplay and
visual reference for the Frisbee mini-game.
Regarding the visual style of the non-player characters, Zeca Bigodes and Carny Wise
were designed to evoke a style reminiscent of the host character from the Carnival Games video
game series, favoring a stylized, cartoon-like aesthetic suited to the rehabilitation context.
Both characters, along with their respective dialogue box layouts, were generated using the
image creation feature provided by Gemini Pro (Google).
The Frisbee mini-game drew direct inspiration from the disc dog activity featured in
Wii Sports Resort (Nintendo, 2009), in which the player throws a frisbee towards designated
areas for a dog to retrieve. This reference also informed the initial low-poly, rounded art style
## 39

adopted for that mini-game’s assets, which was subsequently generalized to the entire project
to ensure visual consistency across all environments and interactive elements.
The majority of the 3D models used in the environment were sourced from Poly Pizza,
an online repository of free, low-poly assets. Table 3.5 lists each model, its author, and the
corresponding attribution link.
A set of 3D models was contributed by Ana Rita Guimarães, a colleague from the Virtual
and Augmented Reality course at Universidade de Aveiro, whose work was developed as part
of the same prototype used in the first wave of user testing (Section 4.1). Her contributions
include the 3D models of the bow, arrows, and balloons, the core interaction mechanics of the
Archery mini-game specifically, drawing and releasing the bowstring to fire arrows and the
balloon burst behavior upon arrow impact as well as the Ferris wheel rotation animation and
its cabin attachments.
Table 3.5: Assets sourced from Poly Pizza, with respective authors and attribution links.
AssetAuthorLink
Sheep ModelGooglepoly.pizza/m/dXBMV4AY2DL
Ferris Wheel ModelGooglepoly.pizza/m/5KiVEnXN5Cw
Tent ModelGooglepoly.pizza/m/9Ob6OO8HMjX
## Fair  Animals  Models
and Animations (Bun-
dle)
Quaterniuspoly.pizza/bundle/Animated-Animal-
Pack-ILAPXeUYiS
Persons Models and An-
imations (Bundle)
J-Toastiepoly.pizza/bundle/CUTES-Part-One-
WD91WrT0gx
Carnival Booth Model   sirkitreepoly.pizza/m/4NizXJZsuO2
Merry Go-Round Modelsirkitreepoly.pizza/m/8BliGG-e55g
Ground ModelAdam Tomkinspoly.pizza/m/achm-Cr9Rr3
Big Tent ModelIan MacGillivray   poly.pizza/m/0TmXixFN81A
Popcorn Cart ModelDon Carsonpoly.pizza/m/1T9untmohmj
Bench ModelSammypoly.pizza/m/3ytbP2GLb0b
SkyBox TextureBorodarassetstore.unity.com/.../farland-skies-
cloudy-crown-60004
The 3D button models used across the interface were sourced from the MetaXR SDK.
The remaining assets the frisbee, the dog, and the ambient environment props (trees, rocks, and
bushes) were modelled and rigged by the author (Figure 3.17). The dog model additionally
received custom Idle and Walk animations, also authored from scratch using Blender’s
armature system.
## 40

Figure 3.17:3D assets modelled and rigged by the author in Blender: theNPCs, comprising
the original character model sourced from Poly Pizza (left) and its modified version
retextured to adopt a carnival worker aesthetic (center-top), the frisbee model (right-
top); the ambient environment props, comprising trees, bushes, and rocks (bottom-left)
and the dog model with its full rig, used to drive the custom Idle and Walk animations
developed for the Frisbee mini-game (bottom-right).
## 41

## CHAPTER
## 4
## User Study
This chapter describes the assessment moments of the proposed personalized Virtual
## Reality (
VR) based country fair, reports the main insights collected, and discusses the
results in light of the topic being addressed.
## 4.1  First Wave: Informal Usability Testing
During the early development phase, informal usability testing was conducted as part of a
virtual and augmented reality course at University of Aveiro, during which students
and teachers had the opportunity to interact with the prototype and provide feedback. Across
the demonstration sessions, approximately six to eight individuals engaged with the system.
The primary objectives of this first wave of testing were threefold: to evaluate whether
the prototype successfully conveyed the intended vibrant “Country Fair” environment, to
evaluate overall enjoyment and engagement.
## 4.1.1  Prototype State
At the time of this evaluation, the prototype was in an early functional stage as shown in
Figure 4.1. The following describes its state, providing context for the scope and limitations
of the test:
•Game Environment: The game environment was already populated with Non-
Playable Characters (NPCs) and thematic fair elements (e.g., tents, ferris wheel
and balloons), establishing the intended aesthetic;
•Fair Scene: This scene was already implemented with the tents to choose the mini-
games;
- Mini-Games: Two mini-games (archery and frisbee) were implemented;
- Game Objectives: Neither mini-game had a defined end state;
•Scoring: In the balloon archery mini-game, balloons of different colors carried different
point values (yellow: 3 points, red: 2 points, blue: 1 point);
## 42

•HUD: The in-game Heads-Up Display (HUD) was rendered at a fixed world-space
position, rather than being anchored to the player’s field of view;
## •
Personalization Module: Not yet implemented, and neither had adaptive parameters
been defined nor integrated;
## •
Cheatcodes: To accelerate iterative testing without requiring a full playthrough, a
set of keyboard shortcuts was introduced at this stage. With the exception of the
emotion representation commands (positive/negativeandf(expression_name)), all
cheatcodes listed in Table 3.1 were implemented at this point;
- Emotion Recognition Module: Not yet implemented.
Figure 4.1:Screenshots of the prototype evaluated during the first wave of testing: the Country Fair
scene (top), the Archery mini-game (bottom left), and the Frisbee mini-game (bottom
right).
## 4.1.2  Experimental Setup
Participants interacted with the game from a seated position using the Meta Quest 3
Head-Mounted Display (HMD), the primary hardware defined during the development
phase.  To allow for clearer perception of the game’s 3D spatial audio and to maximize
immersion, users were equipped with Sony WH-CH500 wireless headphones, as shown in
## Figure 4.2
## 43

Figure 4.2:First Wave informal usability session: two participants interacting with the Country
Fair VR prototype from a seated position using the Meta Quest 3HMDand Sony
WH-CH500 wireless headphones.
## 4.1.3  Measurements
Prior to the sessions, a set of environmental checks was performed, including verifying
lighting conditions and ensuring participants had sufficient space to move freely without risk
of collision. Throughout the sessions, an observer monitored interactions and systematically
recorded any usability issues or difficulties encountered.
## 4.1.4  Procedure
This evaluation was carried out at the Virtual and Augmented Reality Research
## Group (
VAR Lab) at Institute of Electronics and Informatics Engineering of
Aveiro (IEETA), in the scope of the Virtual and Augmented Reality course, involving both
students and teachers as participants. The assessment of the prototype followed a structured
sequence of steps:
•Setup: Initial preparation of the room and configuration of the required equipment,
including the HMD and headphones;
•Briefing: A brief introduction was provided, covering the project’s context, the game’s
objectives, and the specific metrics to be evaluated during the session;
- Gameplay: Each participant interacted with the game prototype under observation;
- Debriefing: Upon completing the gameplay session, the observer invited participants
to share their overall experience, suggestions, and feedback.
4.1.5  Results and Feedback
Participants were able to fulfill the proposed objectives and successfully use the system,
exploring both mini-games without significant difficulty. Observations indicated that users
were visibly engaged throughout the experience, interacting with the system with little to no
assistance required. No signs adverse effects, such as cybersickness, were observed, confirming
the safety of the interaction design.
## 44

Following the sessions, participants provided qualitative feedback that was categorized to
guide iterative refinement of the prototype:
## •
Overall Experience: The mini-games were considered fun to play, and participants
reported feeling immersed in the “Country Fair” environment. The auditory feedback
was particularly appreciated; however, the absence of an in-game tutorial was strongly
noted as a limitation;
## •
VR Frisbee Feedback: One participant noted that the frisbee curved excessively
towards the throwing hand, and felt the dog’s retrieval mechanic was slightly unnatural,
suggesting the dog should deliver the frisbee directly to the nearest outstretched hand;
•VR Archery Feedback: A physical glitch was reported in which the arrow would
sometimes launch while the bowstring was still being drawn. Additionally, using textures
or legend plates to distinguish scoring balloons rather than relying solely on color —
was suggested.
Overall, this first wave provided an initial understanding of key aspects such as usability,
interaction quality, and participant reaction to the Country Fair approach, informing the
refinements carried out prior to the second wave of user testing.
4.2  Second Wave: Formal Usability Testing Without the Target Audience
After developing a robust prototype with integrated personalization features, a formal
usability test was conducted with 38 participants. The primary objectives of this testing
phase were:
1.To determine whether participants noticed the automatic personalization system in action
and if they felt the game’s difficulty adapted appropriately based on their performance;
- To evaluate the user experience, usability, interaction and design of the system;
3.To verify other critical factors before proceeding with the target audience (stroke
survivors), such as whether the mini-games’ instructions and goals were clear and easy to
understand, and whether participants experienced physical fatigue or felt overwhelmed
during gameplay;
4.To review some of the core metrics from the previous testing wave (i.e., whether
participants felt they were at a country fair, and whether they felt motivated, engaged,
and had fun while playing).
## 4.2.1  Prototype State
The prototype tested in this wave (Figure 4.3) incorporated a set of targeted refinements
driven by the feedback gathered during the first wave. Addressing the participant’ feelings
of being lost, a comprehensive tutorial and practice feature was implemented before each
mini-game. The physical glitch identified in the archery mini-game, where the arrow would
prematurely launch while the string was still being pulled, was debugged and successfully
resolved. Furthermore, the feedback regarding the archery scoring system led to a significant
therapeutic improvement: instead of adopting the complex suggestion of using textured
## 45

balloons, the system was redesigned to incorporate a new cognitive rule whereby the player
must shoot a balloon matching a specifically requested color. This design choice not only
resolved the scoring ambiguity but also elevated the mechanic into an engaging cognitive
exercise targeting selective attention, suitable for stroke survivors [20].
Beyond these corrections, the following describes the state of the prototype at the time of
this evaluation:
## •
Game Environment: The fair environment was already populated withNPCs, fair
workers, and fair animals, alongside additional thematic props (benches, tents, etc.). A
post-processing effect was also applied to the virtual environment, elevating contrast
and saturation levels to reinforce the carnival aesthetic;
## •
Player Hands: For theVRrepresentation of the player’s hands, the texture was
replaced with a more realistic one instead of the default shadow hands provided by the
Meta Extended Reality (XR) Software Development Kit (SDK), in order to
provide a more immersive experience for the participants;
## •
Fair Scene: The fair scene included the game introduction sequence delivered by Zeca
Bigodes, the mini-game selection tents, and the motivational congratulatory messages
displayed upon completing a mini-game, at which point the player is teleported back to
the fair;
•Game Objectives: Both mini-games had a defined end condition: the player was
required to reach a predefined target score to complete the session;
•Scoring: Both mini-games already featured their final scoring system, as defined in
## Table 3.3;
- HUD: The in-game HUD was still rendered at a fixed world-space position;
## •
Personalization Module: Fully implemented as described in Subsection 3.3.1, with
the exception that the text feedback was rendered at a fixed world-space position rather
than being anchored to the player’s field of view;
•Emotion Recognition Module: Implemented with the emoji representation form
only, the facial expression and slider representations were not yet integrated. This
module was not actively used during this testing wave.
## 46

Figure 4.3:Screenshots of the prototype evaluated during the second wave of testing: the Country
Fair scene (top), the Archery mini-game (bottom left), and the Frisbee mini-game (bottom
right).
## 4.2.2  Experimental Setup
The experimental setup was highly similar to the one used in the first wave, with the
main differences being the audio device and the inclusion of a peripheral keyboard. While
headphones were mandatory in the first test to verify the 3D spatial audio functionality, their
use was optional in this phase. Furthermore, the Sony WH-CH500 wireless headphones
were replaced by a Steel Nova 3 Wireless headset, as this newer device provides superior
3D spatial audio quality.
Additionally, a Bluetooth keyboard was connected to theHMD. This keyboard was
not intended for player interaction rather, it allowed the observer to input ’cheat codes’.
For example, these codes were used to skip tutorials for participants who had already watched
others play and felt confident proceeding, or to manually force a change in the mini-games’
difficulty. Manually triggering difficulty changes was crucial for this test, as the algorithm’s
default adaptation might perfectly match a user’s performance, which would prevent them
from experiencing and reporting on noticeable difficulty shifts.
## 4.2.3  Tasks
To address this test objectives, participants were asked to perform a sequence of three
specific tasks within the VR environment. These tasks consisted of selecting a mini-game,
completing the frisbee mini-game, and completing the archery mini-game.
## 47

## 4.2.4  Measurements
Before the gameplay sessions began, certain measurements were taken, such as checking
the room’s lighting and ensuring that participants would not hit any surrounding objects while
playing. During the sessions, participants were continuously observed by one to two observers,
who recorded any difficulties encountered and verbal comments made. Afterward, participants
completed a comprehensive questionnaire. The questionnaire began with a brief description
of its objectives, assuring participants that all collected data would be used exclusively for
academic purposes. It primarily consisted of mandatory multiple-choice questions with seven
options using a Likert scale. These questions were organized into the following sections:
Demographic Data (e.g., gender, age, prior experience withVR), About Personalization,
Engagement & Motivation Comfort & Trust and Feedback & Understanding..
## 4.2.5  Procedure
The majority of the user tests were carried out at theVAR LabatIEETA, while some
were conducted in private homes. The evaluation of the prototype followed a structured and
logical process based on the following steps:
•Setup: Initial preparation of the room and configuration of the required equipment
(HMD, headphones, and the Bluetooth keyboard);
•Consent: The participant agreed to take part in the experiment and provided informed
consent, including permission to be photographed during gameplay;
•Briefing: A short introduction was provided regarding the project’s context, the game’s
objectives, and the specific metrics being evaluated during the test;
- Gameplay: The participant tested the game prototype;
•Debriefing: Upon completing the gameplay session, the participant answered a post-
experience questionnaire.
Additionally, the questionnaire included two optional sections: an Acceptance section,
which asked whether participants felt the product was suitable for stroke rehabilitation, and
Effectiveness & Perceived Value section where participants were asked to put themselves in
the shoes of a stroke survivor and indicate whether they believed this personalized experience
could support their rehabilitation progress. Both sections were made non-mandatory for the
same reason, some participants without medical backgrounds might lack the necessary clinical
knowledge to answer confidently.
Finally, a Suggestions or Feedback section provided three open-ended questions for partici-
pants to share qualitative suggestions, general impressions of the game, and impressions of
the personalized XR experience.
As previously mentioned, assessing the system’s usability was a primary goal. To achieve
this, the mandatory System Usability Evaluation section incorporated the System Usability
Scale (SUS), a standard measuring instrument consisting of 10 generic statements that
enable a robust evaluation of the application’s usability, specifically focusing on aspects such
as effectiveness, efficiency, and overall user satisfaction. Since each testing session already
## 48

comprised a gameplay period followed by the completion of the questionnaire, theSUSwas
the only standardized instrument administered, in order to keep the sessions from becoming
too long for participants.
## 4.2.6  Participants
A total of 38 participants were recruited for the study, with ages ranging from 21 to 57
years (6 female, 32 male). Among them, 17 had prior experience with VR.
The majority of participants were students (32 out of 38), most of whom were studying
computer science related fields, including two who held a research grant inXRdevelopment.
Of the remaining six participants, one was unemployed, and the other five came from
healthcare and technology backgrounds: two nurses, one personal trainer, and two professionals
from the IT area specifically, anXRtechnology university professor and an Artificial
Intelligence (AI) engineer. Their diverse expertise was valuable in ensuring well-rounded
feedback.
Figure 4.4 shows 15 of these participants playing the game prototype.
Figure 4.4:Second Wave formal usability study: a representative sample of 15 of the 38 participants
interacting with the Country Fair VR prototype from a seated position, evaluating
system usability, perceptibility of the Dynamic Difficulty Adjustment (DDA)
personalization system, and overall engagement with the “Country Fair” environment.
## 49

## 4.2.7  Data Analysis
To support the analysis of the collected data namely results, suggestions, feedback, and
observations Python scripts were developed with the assistance of generativeAItools. These
scripts use regular expression (regex) pattern matching to automatically classify free-text
responses into predefined thematic categories, allowing the review process to be carried out
more efficiently. The automatically generated outputs were subsequently subject to manual
verification, in order to minimize categorization errors. The analysis of the quantitative data
was primarily descriptive, focusing on response frequencies and the aggregatedSUSscore. No
inferential or correlational analyses were performed, as the study was designed as a usability
evaluation rather than a hypothesis-driven experiment.
4.2.8  Observations and Results
During the testing sessions, observers recorded notable behaviors and verbal comments
expressed by participants, providing valuable qualitative insights that complement the quanti-
tative questionnaire data.
The most notable observation was that 26 participants (68%) explicitly expressed
a desire to keep playing after the formal test had ended.  This spontaneous willingness
to continue engaging with the prototype strongly corroborates the questionnaire findings
regarding motivation and engagement, suggesting that the rehabilitation game successfully
avoids the monotony often associated with conventional therapeutic exercises.
Regarding the primary objective of evaluating the personalization system, the results
were overwhelmingly positive. When asked whether they noticed the system adapting the
mini-games during the session, 31 out of 38 participants (82%) responded positively,
while only 5 (13%) responded negatively and 2 remained neutral. Furthermore, regarding
the statement “The system adapted the mini-games to my specific abilities and needs”, 33
participants (87%) expressed agreement, with only 1 disagreement and 4 neutral responses.
These findings confirm that the dynamic difficulty adjustment mechanism was functional and
perceptually salient to the vast majority of users, fulfilling a core design requirement of the
prototype. It should be noted, however, that these results capture the stated perception of
adaptation rather than its measured impact on performance or experience, isolating the latter
would require a controlled comparison against a non-adaptive condition.
Beyond personalization, the results also validated several other critical factors. Regarding
interface clarity, 31 participants (82%) agreed that “The instructions and objectives of
the mini-games were clear and easy to understand”, with only 2 disagreements and 5 neutral
responses, indicating that the newly implemented tutorial system effectively addressed the
confusion reported during the first wave of testing. Physical comfort was confirmed as a
non-issue: 37 participants (98%) did not feel overly tired or overwhelmed during the session,
with only one negative response a particularly important outcome given that prolonged sessions
are often necessary in a rehabilitation context.
Environmental immersion and motivational quality also received strong endorsement. 34
participants (90%) agreed that they felt immersed in a country fair environment, with
## 50

the remaining 4 remaining neutral and no disagreements recorded. Most importantly, 37
participants (98%) responded positively to the statement The country fair environment
created a fun, motivating, and engaging experience”, with only one neutral response. These
results suggest that the Country Fair” aesthetic successfully promoted engagement and
enjoyment, which are essential drivers for adherence in stroke rehabilitation. However, it
is worth noting that immersion should be carefully managed in terms of session duration,
as prolonged exposure to virtual reality may lead to fatigue or discomfort, particularly in
populations with neurological impairments.
The prototype achieved a remarkable averageSUSscore of 82 (out of 100). According
to the evaluation framework proposed by Bangor et al. (2009), this score falls within the
“Excellent” range (80.0–89.9), corresponding to a letter grade of A- to A, well above the
“Good” threshold (70.0–79.9). In practical terms, this rating suggests that users found theVR
rehabilitation game intuitive, easy to use, and well-suited for its intended purpose, with no
significant barriers to interaction. Given that theSUShas been validated as a robust metric
for evaluating immersiveXRapplications in healthcare, this outcome strongly reinforces the
quality of the implemented design and its readiness for subsequent testing with the target
audience [54] [55] [56].
Regarding secondary objectives, the spatial sound feedback item yielded mixed but
generally positive results: 25 participants (65%) responded positively regarding whether
the spatial sound feedback was helpful in guiding my actions, while 11 (30%) remained neutral
and 2 (5%) responded negatively. This result is important as it may help determine whether
this feature could be beneficial in cases where survivors feel lost during gameplay [4].
The non-mandatory sections Acceptance and Effectiveness & Perceived Value yielded
exclusively positive or neutral responses, with no disagreements recorded across any of their
items. For the Acceptance section, 37 participants responded. Regarding I believe that
personalized VR could be helpful for stroke rehabilitation, 32 participants (86%) responded
positively and 5 (14%) remained neutral.  For I believe this experience could serve as
a beneficial complement to traditional rehabilitation programs by reducing monotony, 36
participants (97%) responded positively and 1 (3%) remained neutral.
For the Effectiveness Perceived Value section, 27 participants responded to I believe
that personalized XR could support my rehabilitation progress, with 22 (81%) responding
positively and 5 (19%) remaining neutral. For The personalized experience felt beneficial
for my rehabilitation, 28 participants responded, with 20 (71%) responding positively and
8 (29%) remaining neutral. It is important to note that these participants were not part
of the target population neither stroke survivors nor healthcare professionals and therefore
these results should not be interpreted as clinical evidence. Nevertheless, the consistently
positive responses, with no negative ratings recorded, provide an encouraging indication of the
system’s perceived value and support the relevance of further evaluation with the intended
end users.
Regarding interaction mechanics, 13 participants (34%) experienced difficulty with
the frisbee mechanics, particularly in picking up, releasing, and throwing the disc. While
## 51

the underlying physics simulation was correctly implemented, the dissipative forces (e.g., air
resistance) are handled automatically by Unity’s physics engine, and the initial throw velocity
is calculated by the Meta
XR SDKbased on the user’s hand movement. Since the developer
lacks full direct control over these components, their interaction can introduce variability
perceived as instability particularly relevant in rehabilitation, where consistent and predictable
mechanics are essential for building patient confidence. By contrast, 9 participants (24%)
reacted positively to the frisbee game, and 14 (38%) to archery.
Furthermore, 4 participants (11%) encountered difficulties with archery mechanics,
primarily related to arrow nocking and string pulling. Notably, 9 participants (24%) did
not notice the adaptive color-based scoring rule or the color change mid-game, suggesting that
the visual indicator within theHUDwas not positioned optimally, or that the relationship
between difficulty changes and balloon color changes was not sufficiently apparent. Technical
issues such as bugs and crashes were observed in 7 participants (18%).
Suggestions and Feedback
From the 38 participants, 31 provided written suggestions or feedback regarding
their experience with the prototype. These qualitative responses were analyzed and categorized
into key themes, offering valuable insights for future iterative refinements.
The most prevalent theme, mentioned by 21 participants (68%), was that the game was
praised as being fun, engaging, and intuitive to play. Closely related, 9 participants (29%)
expressed a positive reaction to the immersive quality of the “Country Fair” environment,
reinforcing the success of the game’s aesthetic design.
Regarding specific game mechanics, the most frequent point of criticism concerned the
frisbee mechanics, which 11 participants (35%) described as awkward, inconsistent, or
physically unintuitive. Furthermore, 7 participants (23%) reported that on-screen text
or instructions were hard to read or too lengthy, and 3 participants (10%) noted that
the adaptive color-based scoring rule for archery was not obvious or easy to notice. These
issues suggest that text elements were not positioned correctly or that the font was not
properly adapted forHMDs, resulting in a slightly pixelated appearance. Proposed solutions
included adding audio narration or voice-over for instructions, and placing the
HUDand text
boxes closer to the player. A further mechanical issue, reported by 5 participants (16%),
concerned difficulty pressing in-game buttons, likely due to inconsistent spacing between them
and imprecise capture of the poke gesture (Figure 3.10) for some users.
4 participants (13%) suggested the addition of more mini-games to enhance engagement,
proposing examples commonly found at country fairs such as a duck picking game or a fishing
game. Regarding personalization specifically, 2 participants (5%) suggested adding more
suggestive audio cues when difficulty changes (e.g., bell or level-up sounds), and another 2
(5%) proposed that maintaining a scoring streak could grant bonus points. One participant
recommended that the frisbee mini-game display a trajectory line similar to archery, which
could disappear at higher difficulty levels, while another made the same suggestion for the
archery game itself. Additionally, one participant suggested providing explicit feedback on
difficulty progression at the end of the session, such as a high-score comparison or an in-game
## 52

indicator rewarding improved performance.
## 4.2.9  Limitations
Despite the encouraging results, this evaluation is not without limitations. Regarding
interaction design, in the Archery mini-game the virtual bow is currently attached to the
player’s left arm, with the right hand performing the grab gesture to pull the string. This
VRinteraction is not optimal for a rehabilitation scenario, since stroke survivors
may be paralyzed in their left arm. This configuration was implemented for prototype
purposes only. Despite this known limitation, the mini-game remains therapeutically validated
by the upper limb motor and cognitive benefits discussed in Subsection 3.4.2, which are
independent of the arm holding the bow.
More critically, both formal waves of testing were conducted exclusively with a non-
clinical population healthy adults, predominantly students and technology professionals.
Although the diversity of expertise provided well-rounded feedback, the absence of the true
target population post-stroke survivors represents the most significant limitation of the
present evaluation. The planned clinical validation with survivors at the Rovisco Pais
Rehabilitation Center, the primary partner institution for this project, could not be carried
out within the scope of this dissertation due to logistical and timeline constraints. Consequently,
while the prototype has demonstrated strong usability and motivational properties with a
general-population substitute, its therapeutic efficacy, accessibility for motor-impaired users,
and clinical safety profile remain empirically unvalidated.
## 4.3  Parallel Study: Emotion Visualization Evaluation
The three emotional representation formats integrated into the Emotion Recognition
module described in Section 3.5 were the subject of a dedicated comparative evaluation
conducted in parallel by a colleague from the same research group (Bianca Guerreiro), carried
out at theVAR LabatIEETA, as part of her master’s dissertation. For this evaluation, the
author contributed by integrating the PersonalizedVRCountry Fair as a client of the emotion
recognition pipeline, adapting the in-game user interface to support the three representation
formats under comparison, and accompanying the testing sessions as an observer, ensuring
the correct functioning of the integrated system throughout.
The study aimed to evaluate how the emoji-based, slider-based, and facial-expression-
based representations were perceived by users in terms of usability, ease of interpretation,
social-emotional connection, and perceived accuracy. A total of 17 participants rated all three
formats within a within-subjects design, using a 7-point Likert scale across 14 items grouped
into four analytical dimensions.
## 4.3.1  Prototype State
The prototype evaluated in this parallel study is the final version, as illustrated in Figure
3.5, incorporated a set of targeted refinements driven by the feedback and observations
gathered during the second wave of testing. Addressing the reported difficulty in reading
## 53

on-screen text and noticing the adaptive color-based scoring rule, theHUDwas redesigned to
follow the player’s head movement, ensuring that all relevant information remained consistently
within the field of view. Furthermore, to improve the visibility of the target color rule in the
archery mini-game, theHUDtext indicating the required balloon color is now rendered in
its corresponding color for example, if the target color is blue, the text is displayed in blue
reinforcing the visual association between the scoring rule and the balloon to hit. Additionally,
a recurring edge-case bug in the archery mini-game, where the arrow after launch would leave
the play area and fail to return to the bow, was identified and resolved.
Beyond these corrections, the following describes the state of the prototype at the time of
this evaluation:
## •
Personalization Module: The sole change relative to the previous wave was anchoring
the text feedback to the player’s field of view. This adjustment ensures that difficulty
change notifications are always visible to the player, regardless of their head orientation,
thereby reinforcing the perceptibility of theDDAsystem and reducing the risk of users
missing adaptation events;
•Cheatcodes:    Implementation  of  the  cheatcodes:(positive/negativeand
f(expression_name)), see Table 3.1;
## •
Emotion Recognition Module: The slider and facial expression forms of emotion
representation were implemented, complementing the emoji representation already
present in the previous wave.
## 4.3.2  Results
Results indicated that all three formats were generally well received, with mean scores
above the neutral midpoint across all dimensions. The emoji-based and slider-based
representations performed comparably regarding usability and ease of interpretation, while
the facial-expression-based format obtained notably higher scores in the social-emotional
connection dimension, particularly in empathy and perceived connectedness. User preferences
were almost evenly distributed across the three formats, with the slider-based and facial-
expression-based representations each selected as the favorite by 6 participants, and the emoji-
based representation by 5. Qualitative feedback further suggested that future implementations
could benefit from more integrated representations, such as avatars or hybrid formats combining
interpretability with social richness.
From the author’s own observations during the sessions, theHUDrefinements introduced
in the present work particularly the text indicating the required balloon color in the archery
mini-game proved effective in practice. Of the 17 participants, only 2 failed to identify the
correct target color during gameplay, one of whom had removed their prescription glasses
during the session, which may account for the reduced ability to perceive the on-screen text.
Overall, these findings mitigate the integration approach adopted in the present work: by
exposing the emotional state as a structured output consumable by external clients, the
Country Fair VR system was designed to remain agnostic to the specific representation
format, allowing results such as these to directly inform future iterations without requiring
architectural changes to the core system.
## 54

## 4.4  Summary
Taken together, the two evaluation waves document the iterative maturation of Country
Fair VR from an early-stage usability prototype to a system ready for clinical validation. The
first wave identified critical interaction issues principally the absence of tutorials and premature
arrow-launch behavior all of which were systematically resolved before the principal evaluation.
The second wave, conducted with 38 participants, provided the core empirical evidence of the
dissertation: the
DDApersonalization system was both functionally correct and perceptually
salient (82% of participants reported noticing adaptation in action); the prototype achieved an
SUSscore of 82 (“Excellent” category) and 98% of participants endorsed the motivational
and immersive quality of the environment. A complementary emotion-visualization study, in
which the author participated as an observer, further demonstrated that all three representation
formats integrated into the Emotion Recognition Module were well received, with the
facial-expression-based format standing out in social-emotional connection, and preferences
distributed evenly across the three methods. These results are now interpreted in terms of
the broader research goals and theoretical commitments of this dissertation in Chapter 5.
## 55

## CHAPTER
## 5
Conclusion and Future Work
## 5.1  Conclusion
This dissertation presented Country Fair VR, a personalized Virtual Reality (VR) serious
game prototype designed to support the upper limb and cognitive rehabilitation of stroke
survivors a response to the well-documented gap in conventional rehabilitation identified in
Chapter 1: the monotony and lack of adaptability inherent in traditional therapeutic exercises,
which progressively erodes survivor motivation and engagement, the very pillars upon which
recovery depends.
The system was designed around a conceptual framework that articulates a closed-loop
architecture for personalizedVRrehabilitation (Figure 3.4). This framework contains four
interconnected components: aVREnvironment (the Country Fair serious game scenario),
a Data Hub responsible for comparing current performance against historical session data
and implementing fatigue-check logic, a Personalization Engine that applies rule-based
Dynamic Difficulty Adjustment (DDA) to maintain the survivor within the optimal state
of Flow, and a Monitoring Tool designed for clinical oversight by healthcare professionals.
What was fully implemented is the Personalization Engine, embedded in the prototype as a
hybrid model that combines autonomousDDAwith the ability for a therapist to manually
override difficulty settings via a peripheral keyboard.
Within this framework, the prototype was developed around a vibrant “Country Fair”
virtual environment. Two therapeutically grounded serious mini-games were implemented: an
Archery mini-game, targeting bimanual static upper limb strength, proprioceptive training,
and selective attention through a dual-task color matching paradigm and a Frisbee mini-
game, targeting dynamic elbow extension, wrist coordination, visuospatial integration, and
reaction capability in direct response to the flexor synergy patterns prevalent in post-stroke
motor impairment. Additionally, an Emotion Recognition Module was integrated as
a complementary component not a primary objective of this dissertation, but a proof-of-
concept integration of a colleague’s parallel research. In the updated implementation, a stroke
survivor’s emotional state is captured, categorized as positive, neutral, or negative, and relayed
## 56

to another survivor’sVRenvironment either as an emoji or as a valence slider contributing
to the mitigation of social isolation, a critical concern identified in the clinical literature.
The prototype was evaluated through a structured, iterative user study conducted in two
waves. The first wave served as an informal usability evaluation during early development,
while the second and principal wave constituted a formal usability study with 38 participants
spanning a wide age range and mixed levels of priorVRexperience.  The results were
consistently positive across all evaluation objectives. The personalization system proved
both functionally correct and perceptually salient, with the vast majority of participants
recognizing the adaptive difficulty mechanism. The prototype achieved a mean System
Usability Scale (SUS) score of 82/100 (“Excellent” category), and 98% of participants
described the experience as fun, motivating, and engaging. Notably, observer notes revealed
that 68% of participants spontaneously expressed a desire to continue playing after the
session, providing ecological evidence of intrinsic motivation. The non-mandatory sections on
acceptance and perceived therapeutic value returned exclusively positive or neutral responses,
with no disagreements recorded.
## 5.2  Future Work
The results and limitations identified throughout this dissertation outline a clear agenda for
future research and development. The following directions are proposed as the most impactful
next steps for advancing Country Fair VR into a clinically validated rehabilitation tool:
## •
Clinical validation with stroke survivors. Conducting user studies with post-stroke
survivors Rovisco Pais Rehabilitation Center under a formal clinical protocol is
the most critical next step, as it is indispensable for translating the demonstrated moti-
vational and usability properties of the prototype into evidence of genuine therapeutic
efficacy.
•Configurable session protocol. Currently, the survivor selects which mini-game tent
to visit freely within the fair environment, with no prescribed sequence or repetition
target. Future work should allow healthcare professionals to define, prior to a session,
an ordered sequence of booths and the number of repetitions required to complete each
one.
•Data Hub and session persistence.  The Data Hub defined in the conceptual
framework responsible for comparing current performance against historical session
data and implementing fatigue-check logic was not implemented in this dissertation.
Future work should develop this processing core to enable cross-session adaptation,
where theDDAalgorithm is informed not only by in-session performance but also by
the survivor’s longitudinal recovery trajectory.
•Healthcare professional monitoring dashboard. The Monitoring Tool outlined
in the conceptual framework designed to give clinicians a structured view of each
survivor’s evolution across sessions likewise falls outside the scope of this prototype
and is deferred to future work. This tool should provide longitudinal visualizations of
## 57

score trajectories, difficulty level history, task completion times, and emotion state logs,
enabling data-informed adjustments to the rehabilitation protocol in alignment with the
hybrid oversight model preferred by healthcare professionals.
•Expansion of the mini-game library. Participants suggested the addition of new
mini-games coherent with the Country Fair setting, such as a fishing game which would
target shoulder abduction, elbow extension, and sustained grip and a duck-picking game.
New mini-games should follow the same therapeutic analysis pipeline applied to the
existing ones to ensure clinical relevance.
## .
In summary, Country Fair VR has established a solid technical and experiential foun-
dation for a personalizedVRrehabilitation system. The work ahead represents a natural
and well-defined roadmap toward a system that could meaningfully complement conventional
stroke rehabilitation programs and contribute to the growing body of evidence supporting the
therapeutic value of personalized Extended Reality (XR) serious games.
## 58

## References
[1]V. L. Feigin, M. Brainin, B. Norrving, et al., «World stroke organization: Global stroke fact sheet 2025»,
International Journal of Stroke, vol. 20, no. 2, pp. 132–144, 2025. doi: 10.1177/17474930241308142
[2]P. Langhorne, J. Bernhardt, and G. Kwakkel, «Stroke rehabilitation», The Lancet, vol. 377, no. 9778,
pp. 1693–1702, May 2011. doi: 10.1016/S0140-6736(11)60325-5
[3]I. Figueiredo et al., «Personalized extended reality experiences to enhance the rehabilitation process
of stroke survivors: A scoping review», Computers & Graphics, vol. 133, p. 104 411, Dec. 2025. doi:
## 10.1016/j.cag.2025.104411
## [4]
I. Figueiredo, B. Marques, S. Oliveira, P. Amorim, and B. S. Santos, «What do healthcare professionals
need? an online study on personalized virtual reality for stroke recovery at a rehabilitation center», in
2025 IEEE International Symposium on Mixed and Augmented Reality Adjunct (ISMAR-Adjunct), 2025,
pp. 346–353. doi: 10.1109/ISMAR-Adjunct68609.2025.00073
## [5]
I. Figueiredo, B. Marques, S. Oliveira, P. Dias, and B. S. Santos, «Towards adaptive virtual reality
for stroke recovery: A personalized approach to rehabilitation», in 2025 IEEE Conference on Virtual
Reality and 3D User Interfaces Abstracts and Workshops (VRW), 2025, pp. 1046–1050. doi:10.1109/
## VRW66409.2025.00211
[6]J. A. Kleim and T. A. Jones, «Principles of experience-dependent neural plasticity: Implications for
rehabilitation after brain damage», Journal of Speech, Language, and Hearing Research, vol. 51, no. 1,
S225–S239, 2008. doi: 10.1044/1092-4388(2008/018)
## [7]
J. Bernhardt et al., «Agreed definitions and a shared vision for new standards in stroke recovery research:
The stroke recovery and rehabilitation roundtable taskforce», International Journal of Stroke, vol. 12,
no. 5, pp. 444–450, Jul. 2017. doi: 10.1177/1747493017711816
[8]A. Streicher and J. D. Smeddinck, «Personalized and adaptive serious games», in Entertainment
Computing and Serious Games, ser. Lecture Notes in Computer Science, R. Dörner, S. Göbel, M.
Kickmeier-Rust, M. Masuch, and K. Zweig, Eds., vol. 9970, Cham: Springer International Publishing,
2016, pp. 332–377. doi: 10.1007/978-3-319-46152-6_14
## [9]
A. Luc, S. Slatman, G. Everard, H. Huygelier, L. Pitance, and C. Detrembleur, «Knowledge, practices,
and acceptance of physiotherapists regarding the use of immersive virtual reality in clinical practice:
A cross-sectional online survey», Disability and Rehabilitation, pp. 1–17, Feb. 2026. doi:
## 10.1080/
## 09638288.2026.2622898
[10]R. L. Sacco et al., «An updated definition of stroke for the 21st century», Stroke, vol. 44, no. 7,
pp. 2064–2089, Jul. 2013. doi: 10.1161/STR.0b013e318296aeca
[11]S. S. Virani et al., «Heart disease and stroke statistics—2020 update: A report from the american heart
association», Circulation, vol. 141, no. 9, e139–e596, Mar. 2020. doi: 10.1161/CIR.0000000000000757
[12]G. Kwakkel, J. M. Veerbeek, E. E. van Wegen, and S. L. Wolf, «Constraint-induced movement therapy
after stroke», Lancet Neurology, vol. 14, no. 2, pp. 224–234, 2015. doi:10.1016/S1474-4422(14)70160-7
[13]H. Thieme et al., «Mirror therapy for improving motor function after stroke», Cochrane Database of
Systematic Reviews, no. 7, p. CD008449, 2018. doi: 10.1002/14651858.CD008449.pub3
[14]S. M. Hatem et al., «Rehabilitation of motor function after stroke: A multiple systematic review focused
on techniques to stimulate upper extremity recovery», Frontiers in Human Neuroscience, vol. 10, Sep.
- doi: 10.3389/fnhum.2016.00442
## 59

[15]P. Milgram and F. Kishino, «A taxonomy of mixed reality visual displays», IEICE TRANSACTIONS
on Information and Systems, vol. 77, no. 12, pp. 1321–1329, 1994.
[16]J. Jerald, «The vr book: Human-centered design for virtual reality», Oct. 2015. doi:10.1145/2792790
[17]  S. M. LaValle, Virtual Reality. Cambridge: Cambridge University Press, 2023.
## [18]
D. Schmalstieg and T. Höllerer, Augmented Reality - Principles and Practice. Jun. 2016, isbn: 978-
## 0321883575.
[19]M. Speicher, B. Hall, and M. Nebeling, «What is mixed reality?», May 2019. doi:10.1145/3290605.
## 3300767
## [20]
A. Bouatrous, A. Meziane, N. Zenati, and C. Hamitouche, «A new adaptive VR-based exergame for
hand rehabilitation after stroke», Multimedia Systems, vol. 29, no. 6, pp. 3385–3402, Dec. 2023. doi:
## 10.1007/s00530-023-01180-0
[21]S. Bae and H.-S. Park, «Development of immersive virtual reality-based hand rehabilitation system
using a gesture-controlled rhythm game with vibrotactile feedback: An fnirs pilot study», IEEE
Transactions on Neural Systems and Rehabilitation Engineering, vol. 31, pp. 3732–3743, 2023. doi:
## 10.1109/TNSRE.2023.3312336
[22]J. J. Rivas et al., «Unobtrusive inference of affective states in virtual rehabilitation from upper limb
motions: A feasibility study», IEEE Transactions on Affective Computing, vol. 11, pp. 470–481, 2018.
doi: 10.1109/TAFFC.2018.2808295
[23]J. Alvarez and D. Djaouti, «An introduction to serious game definitions and concepts», In Proceedings
of the Serious Games Simulation for Risks Management Workshop, pp. 11–15, Jan. 2011.
[24]Z. Luo et al., «Gamification of upper limb virtual rehabilitation in post stroke elderly using silvertune- a
multi-sensory tactile musical assistive system», May 2021, pp. 149–155. doi:10.1109/ICVR51878.2021.
## 9483850
[25]P. Medeiros, R. Capistrano, M. A. Zequinão, S. A. D. Silva, T. S. Beltrame, and F. L. Cardoso,
«Exergames as a tool for the acquisition and development of motor skills and abilities: A systematic
review», Revista Paulista de Pediatria, vol. 35, no. 4, pp. 464–471, Oct. 2017. doi:10.1590/1984-
## 0462/;2017;35;4;00013
## [26]
J. F. Villada Castillo et al., «Design of virtual reality exergames for upper limb stroke rehabilitation
following iterative design methods: Usability study», JMIR Serious Games, vol. 12, e48900, Jan. 2024.
doi: 10.2196/48900
[27]B. Ferreira and P. Menezes, «An adaptive virtual reality-based serious game for therapeutic rehabilita-
tion», International Journal of Online and Biomedical Engineering (iJOE), vol. 16, p. 63, Apr. 2020.
doi: 10.3991/ijoe.v16i04.11923
[28]R. Lopes and R. Bidarra, «Adaptivity challenges in games and simulations: A survey», Computational
Intelligence and AI in Games, IEEE Transactions on, vol. 3, pp. 85–99, Jul. 2011. doi:10.1109/TCIAIG.
## 2011.2152841
## [29]
I. Figueiredo et al., «Comparing methods for integrating personalized eXtended Reality in stroke
rehabilitation», in EUSSET Conference on Computer-Supported Cooperative Work (ECSCW), To be
presented, 2026, pp. 1–10.
[30]M. Csikszentmihalyi, Flow: The Psychology of Optimal Experience. New York: Harper & Row, 1990,
isbn: 9780060162535.
[31]D. E. Guzmán, C. F. Rengifo, J. D. Guzmán, and C. E. Garcia Cena, «Virtual reality games for cognitive
rehabilitation of older adults: A review of adaptive games, domains and techniques», Virtual Reality,
vol. 28, no. 2, p. 92, Apr. 2024. doi: 10.1007/s10055-024-00968-3
[32]S. A. Jackson and H. W. Marsh, «Development and validation of a scale to measure optimal experience:
The flow state scale», Journal of Sport and Exercise Psychology, vol. 18, no. 1, pp. 17–35, 1996. doi:
## 10.1123/jsep.18.1.17
## 60

[33]J. Sinclair, P. Hingston, and M. Masek, «Considerations for the design of exergames», ECU Publications,
Dec. 2007. doi: 10.1145/1321261.1321313
[34]R. S. Sutton and A. G. Barto, Reinforcement Learning: An Introduction, 2nd ed. Cambridge, MA: MIT
Press, 2018, isbn: 9780262039246.
[35]Y. Liao, A. Vakanski, M. Xian, D. Paul, and R. Baker, «A review of computational approaches for
evaluation of rehabilitation exercises», Computers in Biology and Medicine, vol. 119, p. 103 687, 2020.
doi: 10.1016/j.compbiomed.2020.103687
[36]M. M. T. E. Kouijzer, H. Kip, Y. H. A. Bouman, and S. M. Kelders, «Implementation of virtual
reality in healthcare: A scoping review on the implementation process of virtual reality in various
healthcare settings», Implementation Science Communications, vol. 4, no. 1, p. 67, Jun. 2023. doi:
## 10.1186/s43058-023-00442-2
## [37]
S. M. N. Glegg and D. E. Levac, «Barriers, facilitators and interventions to support virtual reality
implementation in rehabilitation: A scoping review», PM&R, vol. 10, pp. 1237–1251, 2018. doi:10.
## 1016/j.pmrj.2018.07.004
## [38]
B. Guerreiro et al., «Visualization framework for shared emotional awareness in personalized VR for
stroke recovery», in Proceedings of the International Conference on Information Visualisation (IV),
2026, pp. 1–6.
[39]I. Figueiredo et al., «Addressing personalized virtual reality for stroke rehabilitation through a country
fair serious game approach: A proposal», in 2026 IEEE Conference on Virtual Reality and 3D User
Interfaces Abstracts and Workshops (VRW), 2026, pp. 914–920. doi: 10.1109/VRW70859.2026.00170
[40]A. Sarkar and S. Cooper, «Inferring and Comparing Game Difficulty Curves using Player-vs-Level
Match Data», pp. 1–4, Aug. 2019. doi: 10.1109/CIG.2019.8848102
[41]N. Kulkarni, P. Desai, S. Jaiswal, S. Chauthe, and Y. Bazaz, «Endless Runner using Procedural
Content Generation & Real-Time Difficulty Curve Generation», International Journal of Engineering
and Technical Research, vol. 3, no. 5, 2015.
## [42]
T. Dorshorst, G. Weir, J. Hamill, and B. Holt, «Archery’s signature: An electromyographic analysis of
the upper limb», Evolutionary Human Sciences, vol. 4, e25, 2022. doi: 10.1017/ehs.2022.20
[43]A. Mahlovanyy et al., «Characterization of the influence of physical rehabilitation means and special
physical exercises of archery on the sports performance of paralympic athletes», Rehabilitation and
Recreation, no. 15, p. 17, 2023. doi: 10.32782/2522-1795.2023.15.2
## [44]
C.-Y. Chen et al., «The rehabilitative effect of archery exercise intervention in patients with parkinson’s
disease», Parkinson’s Disease, vol. 2023, pp. 1–11, Jun. 2023. doi: 10.1155/2023/9175129
## [45]
A. K. Vas, G. Poskey, M. Rice, M. Scheffler, and L. Cook, «Editorial: Unlocking potential: Cognitive
rehabilitation for individuals with acquired brain injuries», Frontiers in Human Neuroscience, vol. 19,
p. 1 730 344, Nov. 2025. doi: 10.3389/fnhum.2025.1730344
[46]M.-S. Kao and C.-H. Wang, «Impact of frisbee game course on the upper limb motor function of
students with intellectual disabilities», International Journal of Developmental Disabilities, vol. 64,
no. 2, pp. 96–104, Mar. 2018. doi: 10.1080/20473869.2016.1267302
## [47]
V. Koutsobina, V. Zakopoulou, E. Tziaka, and V. Koutras, «Evaluating fine perceptual-motor skills
in children with mild intellectual disability», Advances in Developmental and Educational Psychology,
vol. 3, no. 1, pp. 97–108, Aug. 2021. doi: 10.25082/ADEP.2021.01.003
[48]G. Vontzalidis, S. Mystakidis, A. Christopoulos, and K. Moustakas, «Spatial audio cues in an immersive
virtual reality STEM escape room game: A comparative study», in Immersive Learning Research
Network, ser. Communications in Computer and Information Science, J. M. Krüger et al., Eds., vol. 2271,
Cham: Springer Nature Switzerland, 2025, pp. 317–328. doi: 10.1007/978-3-031-80475-5_23
[49]N. O-larnnithipong, A. Barreto, and F. Abyarjoo, «Impact of binaural 3D sound on navigation within a
virtual environment», in Proceedings of the International Joint Conferences on Computer, Information,
and Systems Sciences, and Engineering (CISSE), Bridgeport, CT, USA, Dec. 2014.
## 61

[50]S. M. Magrum, J. Bird, and E. L. Snow, «Using emoji to ascertain student emotional responses to
technology-based human anatomy instruction», Frontiers in Education, vol. 10, 2025, issn: 2504-284X.
doi: 10.3389/feduc.2025.1528443
## [51]
A. Jing, M. Frederick, M. Sewell, A. Karlson, B. Simpson, and M. Smith, «How visualising emotions
affects interpersonal trust and task collaboration in a shared virtual space», in 2023 IEEE International
Symposium on Mixed and Augmented Reality (ISMAR), 2023, pp. 849–858. doi: 10.1109/ISMAR59233.
## 2023.00100
[52]A. Valente and M. Billinghurst, «Cardiocommxr: Training empathy through gamified emotional regula-
tion», in Extended Abstracts of the CHI Conference on Human Factors in Computing Systems, ser. CHI
EA ’25, Association for Computing Machinery, 2025, isbn: 9798400713958. doi:10.1145/3706599.
## 3720319
## [53]
B. Nagy et al., «Therapy reloaded: Mixed reality games for hand rehabilitation», in 2025 IEEE 22nd
Consumer Communications & Networking Conference (CCNC), Las Vegas, NV, USA: IEEE, Jan. 2025,
pp. 1–6. doi: 10.1109/CCNC54725.2025.10976119
[54]A. Bangor, P. Kortum, and J. Miller, «Determining what individual SUS scores mean: Adding an
adjective rating scale», Journal of Usability Studies, vol. 4, no. 3, pp. 114–123, 2009. doi:
https:
## //dl.acm.org/doi/pdf/10.5555/2835587.2835589
[55]P. Arpaia, E. De Benedetto, L. De Paolis, G. D’Errico, N. Donato, and L. Duraccio, «Performance
and usability evaluation of an extended reality platform to monitor patient’s health during surgical
procedures», Sensors, vol. 22, no. 10, p. 3908, May 2022. doi: 10.3390/s22103908
## [56]
A. Rubio-López, R. García Carmona, L. Zarandieta Román, A. Rubio Navas, A. González-Pinto, and
P. Cardinal-Fernández, «Measuring stress and perceptions for a virtual reality-based pericardiocentesis
procedure simulation for medical training: Usability study», JMIR Serious Games, vol. 13, e68515, May
- doi: 10.2196/68515
## 62

## APPENDIX
## A
## Additional Content
A.1  Websites of Institutions
•Virtual and Augmented Reality Research Group (VAR Lab):https://sites.
google.com/view/varlab/home
•Institute of Electronics and Informatics Engineering of Aveiro (IEETA):
https://www.ieeta.pt/
•Rovisco Pais Rehabilitation Center:https://www.cm-cantanhede.pt/mcsite/
Entidade/2871/Hospital-Rovisco-Pais
## A.2  Online Resources
- PolyPizza: https://poly.pizza/
## A.3  Academic Courses
- Virtual And Augmented Reality Course: https://www.ua.pt/en/uc/12023
A.4  Country Fair VR Prototype Demo Videos
- First Wave of User Tests Version: https://youtu.be/Yz8dQVBYweA
- Second Wave of User Tests Version: https://youtu.be/MWP1WmnOW0U
- Final Version: https://www.youtube.com/watch?v=bDXMD0OPzRI
## A.5  Project Repository
https://github.com/DavidPalricas/CountryFair-VR
## 62

A.6  Second Wave of User Tests Questionnaire
## 63

## 64

## 65

## 66

## 67

## 68

A.7  Second Wave of User Tests SUS BoxPlot
## 69

A.8  Second Wave of User Tests SUS BarChart
A.9  Second Wave of User Tests Tasks Completion BoxPlot
## 70

A.10  Second Wave of User Tests Tasks Completion BarCharts
## 71

## 72