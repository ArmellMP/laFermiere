import { useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { ErreurApi, api } from '../api/client'
import type { Alerte, NiveauAlerte } from '../api/types'
import { useAuth } from '../auth/AuthContext'
import { EtatChargement, useDonnees } from '../components/Chargement'
import { useConfirmer } from '../components/Confirmation'
import { IconeAlerte, IconeCarton, IconeCoche, IconeCloche, IconeHorloge, IconePoussin } from '../components/Icones'
import { Badge, BarreOutils, EnTetePage, Indicateurs, Puces, Vide, useEtatUrl } from '../components/Page'
import { TON_NIVEAU } from '../metier'
import { dateLisible } from '../outils'

type Etat = 'non-lues' | 'toutes' | 'lues'
type FiltreNiveau = 'tous' | NiveauAlerte

/** Lien vers l'élément concerné par l'alerte (bande ou stock). */
function lienAlerte(a: Alerte): string | null {
  if (a.typeAlerte === 'BandeACloturer' || a.typeAlerte === 'MortaliteElevee' || a.typeAlerte === 'PeseeManquante' || a.typeAlerte === 'Temperature') {
    return `/bandes/${a.entiteId}`
  }
  if (a.typeAlerte === 'StockFaible' || a.typeAlerte === 'Peremption') return '/stocks?etat=alerte'
  return null
}

function IconeType({ type }: { type: string }) {
  if (type === 'StockFaible' || type === 'Peremption') return <IconeCarton taille={20} />
  if (type === 'BandeACloturer') return <IconeHorloge taille={20} />
  if (type === 'MortaliteElevee') return <IconeAlerte taille={20} />
  return <IconePoussin taille={20} />
}

export default function Alertes() {
  const { utilisateur } = useAuth()
  const confirmer = useConfirmer()
  const [etat, setEtat] = useEtatUrl<Etat>('etat', 'non-lues')
  const [niveau, setNiveau] = useEtatUrl<FiltreNiveau>('niveau', 'tous')
  const [enMarquage, setEnMarquage] = useState<number | 'toutes' | null>(null)
  const [erreurMarquage, setErreurMarquage] = useState<string | null>(null)
  const [luesIci, setLuesIci] = useState<Set<number>>(new Set())

  const { donnees, erreur, enCours, recharger } = useDonnees(() => api.liste<Alerte>('/Alertes'), [])
  // Les alertes marquées ici changent d'état tout de suite, sans recharger la liste
  const alertes = useMemo(() => (donnees ?? []).map((a) => (luesIci.has(a.id) ? { ...a, estLue: true } : a)), [donnees, luesIci])

  const filtrees = alertes
    .filter((a) => (etat === 'non-lues' ? !a.estLue : etat === 'lues' ? a.estLue : true))
    .filter((a) => niveau === 'tous' || a.niveau === niveau)
    .sort((a, b) => Number(a.estLue) - Number(b.estLue) || b.dateAlerte.localeCompare(a.dateAlerte))

  const nonLues = alertes.filter((a) => !a.estLue)
  const critiques = nonLues.filter((a) => a.niveau === 'Critique').length

  const marquer = async (ids: number[] | 'toutes') => {
    if (!utilisateur) return
    setErreurMarquage(null)
    setEnMarquage(ids === 'toutes' ? 'toutes' : ids[0])
    try {
      await api.post('/Alertes/marquer', {
        alerteIds: ids === 'toutes' ? [] : ids,
        utilisateurId: utilisateur.id,
        marquerToutes: ids === 'toutes',
      })
      setLuesIci((s) => new Set([...s, ...(ids === 'toutes' ? nonLues.map((a) => a.id) : ids)]))
    } catch (err) {
      setErreurMarquage(err instanceof ErreurApi ? err.message : 'Impossible de marquer cette alerte.')
    } finally {
      setEnMarquage(null)
    }
  }

  return (
    <main className="page">
      <EnTetePage
        miettes={[{ libelle: 'Tableau de bord', vers: '/' }, { libelle: 'Alertes' }]}
        titre="Alertes"
        sousTitre="Stocks bas, produits périmés et bandes à clôturer, générés automatiquement."
        actions={
          nonLues.length > 0 && (
            <button className="bouton-contour" disabled={enMarquage !== null} onClick={async () => {
                const ok = await confirmer({
                  titre: `Marquer ${nonLues.length} alerte${nonLues.length > 1 ? 's' : ''} comme lue${nonLues.length > 1 ? 's' : ''} ?`,
                  message: 'Elles quitteront la liste « Non lues ». On ne peut pas les remettre en non lues.',
                  libelleConfirmer: 'Tout marquer comme lu',
                  ton: 'attention',
                })
                if (ok) void marquer('toutes')
              }}>
              <IconeCoche taille={18} /> Tout marquer comme lu
            </button>
          )
        }
      />

      <Indicateurs
        elements={[
          { libelle: 'Non lues', valeur: nonLues.length, icone: <IconeCloche taille={20} />, ton: nonLues.length ? 'soleil' : 'neutre', detail: 'à traiter' },
          { libelle: 'Critiques', valeur: critiques, icone: <IconeAlerte taille={20} />, ton: critiques ? 'danger' : 'neutre', detail: 'non lues' },
          { libelle: 'Total', valeur: alertes.length, icone: <IconeCloche taille={20} />, ton: 'neutre', detail: 'depuis le début' },
        ]}
      />

      <BarreOutils>
        <Puces
          libelle="Filtrer par état"
          valeur={etat}
          onChange={setEtat}
          options={[
            { valeur: 'non-lues', libelle: 'Non lues', compte: nonLues.length, ton: 'soleil' },
            { valeur: 'lues', libelle: 'Lues', compte: alertes.length - nonLues.length },
            { valeur: 'toutes', libelle: 'Toutes', compte: alertes.length },
          ]}
        />
        <Puces
          libelle="Filtrer par niveau"
          valeur={niveau}
          onChange={setNiveau}
          options={[
            { valeur: 'tous', libelle: 'Tous niveaux' },
            { valeur: 'Critique', libelle: 'Critique', ton: 'danger' },
            { valeur: 'Avertissement', libelle: 'Avertissement', ton: 'soleil' },
            { valeur: 'Info', libelle: 'Info', ton: 'ciel' },
          ]}
        />
      </BarreOutils>

      {erreurMarquage && (
        <div className="message message-erreur" role="alert">
          <IconeAlerte />
          <p>{erreurMarquage}</p>
        </div>
      )}

      <EtatChargement enCours={enCours} erreur={erreur} recharger={recharger} squelettes={3} />
      {!enCours && !erreur && (
        <>
          {filtrees.length === 0 ? (
            <Vide
              icone={<IconeCoche taille={28} />}
              titre={etat === 'non-lues' ? 'Tout est en ordre' : 'Aucune alerte'}
              texte={etat === 'non-lues' ? 'Aucune alerte en attente. Les alertes sont recalculées toutes les 6 heures.' : 'Aucune alerte ne correspond à ces filtres.'}
            />
          ) : (
            <ul className="liste-alertes">
              {filtrees.map((a, i) => {
                const lien = lienAlerte(a)
                return (
                  <li key={a.id} className={`alerte ton-${TON_NIVEAU[a.niveau]} ${a.estLue ? 'lue' : ''} apparition`} style={{ animationDelay: `${Math.min(i, 10) * 35}ms` }}>
                    <span className={`alerte-icone ton-${TON_NIVEAU[a.niveau]}`}>
                      <IconeType type={a.typeAlerte} />
                    </span>
                    <div className="alerte-corps">
                      <div className="alerte-haut">
                        <strong>{a.typeAlerteLibelle}</strong>
                        <Badge ton={TON_NIVEAU[a.niveau]}>{a.niveauLibelle}</Badge>
                      </div>
                      <p>{a.message}</p>
                      <div className="alerte-bas">
                        <span className="discret">{dateLisible(a.dateAlerte)}</span>
                        {lien && (
                          <Link to={lien} className="lien-action">
                            Voir
                          </Link>
                        )}
                        {!a.estLue && (
                          <button className="lien-action" disabled={enMarquage !== null} onClick={() => void marquer([a.id])}>
                            {enMarquage === a.id ? 'Marquage…' : 'Marquer comme lue'}
                          </button>
                        )}
                      </div>
                    </div>
                  </li>
                )
              })}
            </ul>
          )}
        </>
      )}
    </main>
  )
}
