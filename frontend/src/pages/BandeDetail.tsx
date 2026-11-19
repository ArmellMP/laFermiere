import { useEffect, useMemo, useState } from 'react'
import { createPortal } from 'react-dom'
import { Link, useLocation, useNavigate, useParams } from 'react-router-dom'
import { api } from '../api/client'
import { avecNomsAchats, avecNomsVentes } from '../api/noms'
import type { Achat, Bande, Stock, Suivi, Vente } from '../api/types'
import { useAuth } from '../auth/AuthContext'
import { useNotifier } from '../components/Formulaire'
import { FormulaireAchat, FormulaireCloture, FormulaireDemarrage, FormulaireVente } from '../formulaires/Formulaires'
import { EtatChargement, useDonnees } from '../components/Chargement'
import { Courbe } from '../components/Graphique'
import {
  IconeBalance,
  IconeCarton,
  IconeCoche,
  IconeCoeur,
  IconeHorloge,
  IconeNote,
  IconePanier,
  IconePlus,
  IconePoussin,
  IconeSeringue,
  IconeThermometre,
  IconeVente,
} from '../components/Icones'
import { Badge, Carte, EnTetePage, Indicateurs, Jauge, Onglets, Vide, useEtatUrl } from '../components/Page'
import { BoutonLigne, BoutonsLigne, TableauAchats, TableauStocks, TableauVentes } from '../formulaires/Gestion'
import { ConfirmationSuppression, type DemandeSuppression } from '../formulaires/Actions'
import { colonnesAchat, colonnesStock, colonnesVente } from '../colonnes'
import { LIBELLE_STATUT, TON_STATUT, ageBande, estActive, joursRestants, tonMortalite } from '../metier'
import { DUREE_CYCLE, aujourdhui, dateLisible, monnaie, nombre, partieDate } from '../outils'

type Onglet = 'apercu' | 'journal' | 'stocks' | 'ventes' | 'achats'

export default function BandeDetail() {
  const { id } = useParams()
  const { aLeRole } = useAuth()
  const location = useLocation()
  const naviguer = useNavigate()
  const [onglet, setOnglet] = useEtatUrl<Onglet>('onglet', 'apercu')
  const [celebration, setCelebration] = useState(() => (location.state as { message?: string } | null)?.message ?? null)

  // Le message de réussite ne s'affiche qu'une fois (pas après un rechargement)
  useEffect(() => {
    if (!celebration) return
    naviguer(location.pathname + location.search, { replace: true, state: null })
    const minuteur = setTimeout(() => setCelebration(null), 4500)
    return () => clearTimeout(minuteur)
  }, []) // eslint-disable-line

  const { donnees, erreur, enCours, recharger } = useDonnees(async () => {
    const [bande, suivis] = await Promise.all([api.get<Bande>(`/Bandes/${id}`), api.get<Suivi[]>(`/SuiviJournalier/bande/${id}`)])
    return { bande, suivis: [...suivis].sort((a, b) => b.dateSuivi.localeCompare(a.dateSuivi)) }
  }, [id])

  const voitVentes = aLeRole('Admin', 'Comptable')
  const voitAchats = aLeRole('Admin', 'Comptable', 'Fermier')
  const notifier = useNotifier()
  const [formulaire, setFormulaire] = useEtatUrl<'' | 'cloture' | 'demarrage'>('formulaire', '')

  if (!donnees) {
    return (
      <main className="page">
        <EnTetePage miettes={[{ libelle: 'Bandes', vers: '/bandes' }, { libelle: '…' }]} titre="Chargement…" />
        <EtatChargement enCours={enCours} erreur={erreur} recharger={recharger} squelettes={3} />
      </main>
    )
  }

  const { bande, suivis } = donnees
  const active = estActive(bande)
  const age = ageBande(bande)
  const morts = bande.nbPoussinsInit - bande.nbPoussinsActuel
  const suiviDuJour = suivis.find((s) => partieDate(s.dateSuivi) === aujourdhui())
  const peutSaisir = active && aLeRole('Admin', 'Fermier', 'Veterinaire')
  const dernierPoids = suivis.find((s) => s.poidsMoyen != null)
  const finPrevue = new Date(partieDate(bande.dateArrivee))
  finPrevue.setDate(finPrevue.getDate() + DUREE_CYCLE - 1)

  const onglets: { valeur: Onglet; libelle: string; compte?: number }[] = [
    { valeur: 'apercu', libelle: 'Aperçu' },
    { valeur: 'journal', libelle: 'Journal', compte: suivis.length },
    { valeur: 'stocks', libelle: 'Stocks' },
    ...(voitVentes ? [{ valeur: 'ventes' as const, libelle: 'Ventes' }] : []),
    ...(voitAchats ? [{ valeur: 'achats' as const, libelle: 'Achats' }] : []),
  ]

  return (
    <main className="page">
      {celebration && <Celebration message={celebration} />}

      <EnTetePage
        miettes={[{ libelle: 'Tableau de bord', vers: '/' }, { libelle: 'Bandes', vers: '/bandes' }, { libelle: bande.code }]}
        titre={bande.code}
        badge={
          <Badge ton={TON_STATUT[bande.statut]} point={active}>
            {LIBELLE_STATUT[bande.statut]}
          </Badge>
        }
        sousTitre={
          <>
            Arrivée le {dateLisible(bande.dateArrivee)} · {nombre(bande.nbPoussinsInit)} poussins
            {active && <> · fin prévue le {dateLisible(finPrevue.toISOString())}</>}
          </>
        }
        actions={
          <>
            {bande.statut === 'EnPreparation' && aLeRole('Admin', 'Fermier') && (
              <button className="bouton-contour" onClick={() => setFormulaire('demarrage')}>
                Démarrer l’élevage
              </button>
            )}
            {bande.statut === 'EnCours' && aLeRole('Admin', 'Fermier') && (
              <button className="bouton-contour" onClick={() => setFormulaire('cloture')}>
                Clôturer
              </button>
            )}
            {peutSaisir &&
              (suiviDuJour ? (
                <span className="etat-fait">
                  <IconeCoche taille={18} /> Suivi du jour {suiviDuJour.jour} fait
                </span>
              ) : (
                <Link to={`/bandes/${bande.id}/suivi`} className="bouton-principal">
                  <IconePlus taille={20} /> Saisir le suivi du jour
                </Link>
              ))}
          </>
        }
      />

      {bande.statut === 'EnPreparation' && (
        <FormulaireDemarrage
          bande={bande}
          ouvert={formulaire === 'demarrage'}
          onFermer={() => setFormulaire('')}
          onCree={(b) => {
            setFormulaire('')
            notifier(`Élevage de la bande ${b.code} démarré`)
            void recharger()
          }}
        />
      )}
      {bande.statut === 'EnCours' && (
        <FormulaireCloture
          bande={bande}
          ouvert={formulaire === 'cloture'}
          onFermer={() => setFormulaire('')}
          onCree={(b) => {
            setFormulaire('')
            notifier(`Bande ${b.code} clôturée`)
            void recharger()
          }}
        />
      )}

      {active && (
        <div className="progression-cycle apparition">
          <div className="progression-cycle-texte">
            <strong>
              Jour {age} <span className="discret">sur {DUREE_CYCLE}</span>
            </strong>
            <span className="discret">{joursRestants(bande)} jours restants</span>
          </div>
          <Jauge valeur={age} max={DUREE_CYCLE} ton={joursRestants(bande) <= 7 ? 'soleil' : 'vert'} />
        </div>
      )}

      <Indicateurs
        elements={[
          { libelle: 'Effectif actuel', valeur: bande.nbPoussinsActuel, icone: <IconePoussin taille={20} />, ton: 'vert', detail: `sur ${nombre(bande.nbPoussinsInit)} au départ` },
          {
            libelle: 'Mortalité',
            valeur: bande.tauxMortalite,
            decimales: 1,
            suffixe: ' %',
            icone: <IconeCoeur taille={20} />,
            ton: tonMortalite(bande.tauxMortalite),
            detail: `${nombre(morts)} sujet${morts > 1 ? 's' : ''} perdu${morts > 1 ? 's' : ''}`,
          },
          {
            libelle: 'Poids moyen',
            valeur: dernierPoids?.poidsMoyen ?? null,
            decimales: 2,
            suffixe: ' kg',
            icone: <IconeBalance taille={20} />,
            ton: 'soleil',
            detail: dernierPoids ? `pesée du jour ${dernierPoids.jour}` : 'aucune pesée',
          },
          {
            libelle: active ? 'Âge de la bande' : 'Durée du cycle',
            valeur: age,
            suffixe: ' j',
            icone: <IconeHorloge taille={20} />,
            ton: 'ciel',
            detail: `${suivis.length} suivi${suivis.length > 1 ? 's' : ''} saisi${suivis.length > 1 ? 's' : ''}`,
          },
        ]}
      />

      <Onglets onglets={onglets} valeur={onglet} onChange={setOnglet} />

      <div key={onglet} className="contenu-onglet">
        {onglet === 'apercu' && <Apercu bande={bande} suivis={suivis} />}
        {onglet === 'journal' && (
          <Journal
            suivis={suivis}
            bandeId={bande.id}
            modifiable={active && aLeRole('Admin', 'Fermier', 'Veterinaire')}
            onChange={() => void recharger()}
          />
        )}
        {onglet === 'stocks' && <StocksBande bandeId={bande.id} />}
        {onglet === 'ventes' && voitVentes && <VentesBande bandeId={bande.id} peutCreer onChange={() => void recharger()} />}
        {onglet === 'achats' && voitAchats && (
          <AchatsBande bandeId={bande.id} peutCreer={aLeRole('Admin', 'Comptable')} onChange={() => void recharger()} />
        )}
      </div>
    </main>
  )
}

// ---------------------------------------------------------------------------
// Onglet Aperçu : courbes + informations + derniers suivis
// ---------------------------------------------------------------------------
function Apercu({ bande, suivis }: { bande: Bande; suivis: Suivi[] }) {
  const chronologique = useMemo(() => [...suivis].sort((a, b) => a.jour - b.jour), [suivis])
  const poids = chronologique.filter((s) => s.poidsMoyen != null).map((s) => ({ x: s.jour, y: s.poidsMoyen! }))
  let cumul = 0
  const mortalite = chronologique.map((s) => {
    cumul += s.nbMortalites
    return { x: s.jour, y: (cumul / bande.nbPoussinsInit) * 100 }
  })
  const soins = suivis.filter((s) => s.typeSoin).length

  return (
    <div className="grille-apercu">
      <Carte titre="Croissance — poids moyen" className="apparition">
        <Courbe points={poids} unite="kg" decimales={2} couleur="var(--soleil-fonce)" />
      </Carte>
      <Carte titre="Mortalité cumulée" className="apparition">
        <Courbe points={mortalite} unite="%" decimales={1} couleur="var(--danger)" />
      </Carte>
      <Carte titre="Fiche de la bande" className="apparition">
        <dl className="fiche">
          <div>
            <dt>Code</dt>
            <dd>{bande.code}</dd>
          </div>
          <div>
            <dt>Statut</dt>
            <dd>{LIBELLE_STATUT[bande.statut]}</dd>
          </div>
          <div>
            <dt>Arrivée</dt>
            <dd>{dateLisible(bande.dateArrivee)}</dd>
          </div>
          {bande.dateCloture && (
            <div>
              <dt>Clôture</dt>
              <dd>{dateLisible(bande.dateCloture)}</dd>
            </div>
          )}
          <div>
            <dt>Poussins reçus</dt>
            <dd>{nombre(bande.nbPoussinsInit)}</dd>
          </div>
          <div>
            <dt>Soins donnés</dt>
            <dd>{soins}</dd>
          </div>
        </dl>
        {bande.observations && <p className="fiche-note">{bande.observations}</p>}
      </Carte>
      <Carte titre="Derniers suivis" className="apparition">
        {suivis.length === 0 ? (
          <p className="discret">Aucun suivi saisi.</p>
        ) : (
          <ul className="liste-compacte">
            {suivis.slice(0, 4).map((s) => (
              <li key={s.id}>
                <span className="liste-compacte-jour">J{s.jour}</span>
                <span className="liste-compacte-texte">
                  {s.nbMortalites === 0 ? 'Aucun mort' : `${s.nbMortalites} mort${s.nbMortalites > 1 ? 's' : ''}`}
                  {s.poidsMoyen != null && ` · ${nombre(s.poidsMoyen, 2)} kg`}
                  {s.typeSoin && ` · ${s.typeSoinLibelle ?? s.typeSoin}`}
                </span>
                <span className="discret">{dateLisible(s.dateSuivi)}</span>
              </li>
            ))}
          </ul>
        )}
      </Carte>
    </div>
  )
}

// ---------------------------------------------------------------------------
// Onglet Journal : frise complète des suivis
// ---------------------------------------------------------------------------
function Journal({ suivis, bandeId, modifiable, onChange }: { suivis: Suivi[]; bandeId: number; modifiable: boolean; onChange: () => void }) {
  const notifier = useNotifier()
  const naviguer = useNavigate()
  const [suppression, setSuppression] = useState<DemandeSuppression | null>(null)

  const demanderSuppression = (s: Suivi) =>
    setSuppression({
      titre: `Supprimer le suivi du jour ${s.jour}`,
      route: `/SuiviJournalier/${s.id}`,
      consequence: [
        s.nbMortalites > 0 && `Les ${s.nbMortalites} mort${s.nbMortalites > 1 ? 's' : ''} seront remis dans l’effectif de la bande.`,
        s.quantiteConsommee && `La quantité consommée sera rendue au stock.`,
      ]
        .filter(Boolean)
        .join(' ') || undefined,
      description: (
        <>
          <strong>Jour {s.jour}</strong> · {dateLisible(s.dateSuivi)} · {s.nbMortalites} mort{s.nbMortalites > 1 ? 's' : ''}
        </>
      ),
    })

  if (suivis.length === 0) {
    return <Vide icone={<IconeNote taille={28} />} titre="Journal vide" texte="Le premier suivi journalier apparaîtra ici." />
  }
  return (
    <>
    <ConfirmationSuppression
      demande={suppression}
      onFermer={() => setSuppression(null)}
      onFait={() => {
        setSuppression(null)
        notifier('Suivi supprimé')
        onChange()
      }}
    />
    <ol className="frise">
      {suivis.map((s, i) => (
        <li key={s.id} className="frise-element apparition" style={{ animationDelay: `${Math.min(i, 10) * 40}ms` }}>
          <span className={`frise-point ${s.nbMortalites > 0 ? 'point-danger' : s.typeSoin ? 'point-soin' : 'point-ok'}`} />
          <div className="frise-carte">
            <div className="frise-haut">
              <strong>Jour {s.jour}</strong>
              <span className="discret">
                {dateLisible(s.dateSuivi)}
                {s.createurNom && ` · ${s.createurNom}`}
              </span>
            </div>
            <div className="frise-puces">
              <Badge ton={s.nbMortalites > 0 ? 'danger' : 'vert'}>
                <IconeCoeur taille={14} />
                {s.nbMortalites === 0 ? 'Aucun mort' : `${s.nbMortalites} mort${s.nbMortalites > 1 ? 's' : ''}`}
              </Badge>
              {s.poidsMoyen != null && (
                <Badge ton="soleil">
                  <IconeBalance taille={14} />
                  {nombre(s.poidsMoyen, 2)} kg{s.nbSujetsPeses ? ` · ${s.nbSujetsPeses} pesés` : ''}
                </Badge>
              )}
              {s.typeSoin && (
                <Badge ton="soin">
                  <IconeSeringue taille={14} />
                  {s.typeSoinLibelle ?? s.typeSoin}
                </Badge>
              )}
              {s.temperature != null && (
                <Badge ton="ciel">
                  <IconeThermometre taille={14} />
                  {nombre(s.temperature, 1)} °C
                </Badge>
              )}
            </div>
            {modifiable && (
              <BoutonsLigne>
                <BoutonLigne libelle="Modifier" onClick={() => naviguer(`/bandes/${bandeId}/suivi/${s.id}`)} />
                <BoutonLigne libelle="Supprimer" danger onClick={() => demanderSuppression(s)} />
              </BoutonsLigne>
            )}
            {(s.causeMortalite || s.produitUtilise || s.observations) && (
              <p className="frise-texte">
                {[s.causeMortalite && `Cause : ${s.causeMortalite}`, s.produitUtilise && `${s.produitUtilise}${s.dosage ? ` (${s.dosage})` : ''}`, s.observations]
                  .filter(Boolean)
                  .join(' · ')}
              </p>
            )}
          </div>
        </li>
      ))}
    </ol>
    </>
  )
}

// ---------------------------------------------------------------------------
// Onglets Stocks, Ventes, Achats : chargés à l'ouverture de l'onglet
// ---------------------------------------------------------------------------

function StocksBande({ bandeId }: { bandeId: number }) {
  const { donnees, erreur, enCours, recharger } = useDonnees(() => api.get<Stock[]>(`/Stocks/bande/${bandeId}`), [bandeId])
  if (!donnees) return <EtatChargement enCours={enCours} erreur={erreur} recharger={recharger} />
  return (
    <TableauStocks
      lignes={donnees}
      colonnes={colonnesStock}
      vide={<Vide icone={<IconeCarton taille={28} />} titre="Aucun stock pour cette bande" texte="Les achats d’aliment et de médicaments créent les stocks." />}
      onChange={() => void recharger()}
    />
  )
}

function VentesBande({ bandeId, peutCreer, onChange }: { bandeId: number; peutCreer: boolean; onChange: () => void }) {
  const notifier = useNotifier()
  const [ouvert, setOuvert] = useState(false)
  const { donnees, erreur, enCours, recharger } = useDonnees(async () => avecNomsVentes(await api.get<Vente[]>(`/Ventes/bande/${bandeId}`)), [bandeId])
  if (!donnees) return <EtatChargement enCours={enCours} erreur={erreur} recharger={recharger} />
  const total = donnees.reduce((s, v) => s + v.montantTotal, 0)
  return (
    <>
      <div className="barre-onglet">
        <p className="resultat-compte">
          {donnees.length} vente{donnees.length > 1 ? 's' : ''} · <strong>{monnaie(total)}</strong>
        </p>
        {peutCreer && (
          <button className="bouton-contour petit" onClick={() => setOuvert(true)}>
            <IconePlus taille={16} /> Nouvelle vente
          </button>
        )}
      </div>
      <TableauVentes
        lignes={donnees}
        colonnes={colonnesVente.filter((c) => c.cle !== 'bande')}
        vide={<Vide icone={<IconeVente taille={28} />} titre="Aucune vente pour cette bande" />}
        onChange={() => {
          void recharger()
          onChange()
        }}
      />
      <FormulaireVente
        bandeId={bandeId}
        ouvert={ouvert}
        onFermer={() => setOuvert(false)}
        onCree={(v) => {
          setOuvert(false)
          notifier(`Vente de ${monnaie(v.montantTotal)} enregistrée`)
          void recharger()
          onChange()
        }}
      />
    </>
  )
}

function AchatsBande({ bandeId, peutCreer, onChange }: { bandeId: number; peutCreer: boolean; onChange: () => void }) {
  const notifier = useNotifier()
  const [ouvert, setOuvert] = useState(false)
  const { donnees, erreur, enCours, recharger } = useDonnees(async () => avecNomsAchats(await api.get<Achat[]>(`/Achats/bande/${bandeId}`)), [bandeId])
  if (!donnees) return <EtatChargement enCours={enCours} erreur={erreur} recharger={recharger} />
  const total = donnees.reduce((s, a) => s + a.montantTotal, 0)
  return (
    <>
      <div className="barre-onglet">
        <p className="resultat-compte">
          {donnees.length} achat{donnees.length > 1 ? 's' : ''} · <strong>{monnaie(total)}</strong>
        </p>
        {peutCreer && (
          <button className="bouton-contour petit" onClick={() => setOuvert(true)}>
            <IconePlus taille={16} /> Nouvel achat
          </button>
        )}
      </div>
      <TableauAchats
        lignes={donnees}
        colonnes={colonnesAchat.filter((c) => c.cle !== 'bande')}
        vide={<Vide icone={<IconePanier taille={28} />} titre="Aucun achat pour cette bande" />}
        onChange={() => {
          void recharger()
          onChange()
        }}
      />
      <FormulaireAchat
        bandeId={bandeId}
        ouvert={ouvert}
        onFermer={() => setOuvert(false)}
        onCree={(a) => {
          setOuvert(false)
          notifier(`Achat de ${monnaie(a.montantTotal)} enregistré`)
          void recharger()
          onChange()
        }}
      />
    </>
  )
}



/** Petite fête après une saisie réussie : coche qui se dessine + confettis. */
function Celebration({ message }: { message: string }) {
  // Portail : au-dessus de tout, quelle que soit l'animation de la page
  return createPortal(
    <div className="celebration" role="status">
      <div className="confettis" aria-hidden="true">
        {Array.from({ length: 14 }, (_, i) => (
          <span key={i} style={{ ['--i' as string]: i }} />
        ))}
      </div>
      <div className="celebration-carte">
        <svg className="coche-animee" viewBox="0 0 52 52" aria-hidden="true">
          <circle cx="26" cy="26" r="24" />
          <path d="M15 27l7 7 15-16" />
        </svg>
        <p>{message}</p>
      </div>
    </div>,
    document.body,
  )
}
