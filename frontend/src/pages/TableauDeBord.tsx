import { Link } from 'react-router-dom'
import { api } from '../api/client'
import { avecNomsStocks, avecNomsVentes } from '../api/noms'
import type { Alerte, Bande, Stock, Vente } from '../api/types'
import { useAuth } from '../auth/AuthContext'
import { EtatChargement, useDonnees } from '../components/Chargement'
import { IconeAlerte, IconeCarton, IconeChevron, IconeCloche, IconeCoche, IconeCoeur, IconePoussin, IconeVente } from '../components/Icones'
import { Badge, Carte, EnTetePage, Indicateurs, Jauge, type Indicateur } from '../components/Page'
import { TON_NIVEAU, ageBande, estActive, finDeCycleProche, joursRestants, tonMortalite } from '../metier'
import { DUREE_CYCLE, aujourdhui, dateLisible, monnaie, monnaieCompacte, nombre } from '../outils'

function salutation(): string {
  const heure = new Date().getHours()
  if (heure < 5 || heure >= 18) return 'Bonsoir'
  return 'Bonjour'
}

export default function TableauDeBord() {
  const { utilisateur, aLeRole } = useAuth()
  const voitVentes = aLeRole('Admin', 'Comptable')

  const { donnees, erreur, enCours, recharger } = useDonnees(async () => {
    const [bandes, alertes, stocksBas, ventes] = await Promise.all([
      api.liste<Bande>('/Bandes'),
      api.get<Alerte[]>('/Alertes/non-lues'),
      api.get<Stock[]>('/Stocks/alerte'),
      voitVentes ? api.liste<Vente>('/Ventes') : Promise.resolve([] as Vente[]),
    ])
    return { bandes, alertes, stocksBas: await avecNomsStocks(stocksBas), ventes: await avecNomsVentes(ventes) }
  }, [voitVentes])

  const date = new Date().toLocaleDateString('fr-FR', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' })
  const prenom = utilisateur?.nom.split(/\s+/)[0] ?? ''

  const entete = (
    <EnTetePage
      titre={`${salutation()} ${prenom}`}
      sousTitre={<span className="capitalise">{date}</span>}
      actions={
        <Link to="/bandes" className="bouton-principal">
          <IconePoussin taille={20} /> Mes bandes
        </Link>
      }
    />
  )

  if (!donnees) {
    return (
      <main className="page">
        {entete}
        <EtatChargement enCours={enCours} erreur={erreur} recharger={recharger} squelettes={4} />
      </main>
    )
  }

  const { bandes, alertes, stocksBas, ventes } = donnees
  const actives = bandes.filter(estActive).sort((a, b) => ageBande(b) - ageBande(a))
  const sujets = actives.reduce((s, b) => s + b.nbPoussinsActuel, 0)
  const initiaux = actives.reduce((s, b) => s + b.nbPoussinsInit, 0)
  const mortalite = initiaux ? ((initiaux - sujets) / initiaux) * 100 : 0
  const critiques = alertes.filter((a) => a.niveau === 'Critique').length
  const mois = aujourdhui().slice(0, 7)
  const ventesDuMois = ventes.filter((v) => v.dateVente.startsWith(mois))
  const caMois = ventesDuMois.reduce((s, v) => s + v.montantTotal, 0)
  const aEncaisser = ventes.filter((v) => !v.estPaye).reduce((s, v) => s + v.montantTotal, 0)

  const indicateurs: Indicateur[] = [
    { libelle: 'Bandes actives', valeur: actives.length, icone: <IconePoussin taille={20} />, ton: 'vert', detail: `${actives.filter(finDeCycleProche).length} en fin de cycle`, vers: '/bandes' },
    { libelle: 'Poulets sur pied', valeur: sujets, icone: <IconePoussin taille={20} />, ton: 'ciel', detail: `sur ${nombre(initiaux)} reçus` },
    { libelle: 'Mortalité moyenne', valeur: mortalite, decimales: 1, suffixe: ' %', icone: <IconeCoeur taille={20} />, ton: tonMortalite(mortalite), detail: 'bandes actives', vers: '/bandes?tri=mortalite' },
    voitVentes
      ? { libelle: 'Ventes du mois', valeur: caMois, format: monnaieCompacte, icone: <IconeVente taille={20} />, ton: 'vert', detail: `${ventesDuMois.length} vente${ventesDuMois.length > 1 ? 's' : ''} · ${monnaieCompacte(aEncaisser)} à encaisser`, vers: '/ventes' }
      : { libelle: 'Stocks bas', valeur: stocksBas.length, icone: <IconeCarton taille={20} />, ton: stocksBas.length ? 'soleil' : 'neutre', detail: 'sous le seuil d’alerte', vers: '/stocks?etat=alerte' },
  ]

  return (
    <main className="page">
      {entete}

      {critiques > 0 && (
        <Link to="/alertes?niveau=Critique" className="bandeau-critique apparition">
          <IconeAlerte />
          <span>
            <strong>
              {critiques} alerte{critiques > 1 ? 's' : ''} critique{critiques > 1 ? 's' : ''}
            </strong>{' '}
            demande{critiques > 1 ? 'nt' : ''} votre attention.
          </span>
          <IconeChevron taille={18} />
        </Link>
      )}

      <Indicateurs elements={indicateurs} />

      <div className="grille-tableau-bord">
        <Carte
          className="zone-principale apparition"
          titre="Bandes en cours"
          action={
            <Link to="/bandes" className="lien-action">
              Tout voir
            </Link>
          }
        >
          {actives.length === 0 ? (
            <p className="discret">Aucune bande en cours.</p>
          ) : (
            <ul className="liste-bandes-compacte">
              {actives.slice(0, 6).map((b) => (
                <li key={b.id}>
                  <Link to={`/bandes/${b.id}`} className="ligne-bande">
                    <div className="ligne-bande-titre">
                      <strong>{b.code}</strong>
                      <span className="discret">
                        {nombre(b.nbPoussinsActuel)} sujets · <span className={`texte-${tonMortalite(b.tauxMortalite)}`}>{nombre(b.tauxMortalite, 1)} %</span>
                      </span>
                    </div>
                    <div className="ligne-bande-cycle">
                      <span>
                        J{ageBande(b)}
                        <span className="discret">/{DUREE_CYCLE}</span>
                      </span>
                      <Jauge valeur={ageBande(b)} max={DUREE_CYCLE} ton={finDeCycleProche(b) ? 'soleil' : 'vert'} />
                      <span className={finDeCycleProche(b) ? 'texte-soleil' : 'discret'}>{joursRestants(b)} j</span>
                    </div>
                  </Link>
                </li>
              ))}
            </ul>
          )}
        </Carte>

        <Carte
          className="apparition"
          titre="À surveiller"
          action={
            <Link to="/alertes" className="lien-action">
              Alertes
            </Link>
          }
        >
          {alertes.length === 0 ? (
            <p className="tout-va-bien">
              <IconeCoche taille={20} /> Aucune alerte en attente
            </p>
          ) : (
            <ul className="liste-compacte">
              {alertes.slice(0, 5).map((a) => (
                <li key={a.id}>
                  <span className={`point-niveau ton-${TON_NIVEAU[a.niveau]}`} />
                  <span className="liste-compacte-texte">{a.message}</span>
                </li>
              ))}
            </ul>
          )}
        </Carte>

        <Carte
          className="apparition"
          titre="Stocks bas"
          action={
            <Link to="/stocks?etat=alerte" className="lien-action">
              Stocks
            </Link>
          }
        >
          {stocksBas.length === 0 ? (
            <p className="tout-va-bien">
              <IconeCoche taille={20} /> Tous les stocks sont suffisants
            </p>
          ) : (
            <ul className="liste-compacte">
              {stocksBas.slice(0, 5).map((s) => (
                <li key={s.id}>
                  <span className="liste-compacte-texte">
                    <strong>{s.nom}</strong>
                    <span className="discret"> · {s.bandeCode}</span>
                  </span>
                  <Badge ton={s.quantiteActuelle === 0 ? 'danger' : 'soleil'}>
                    {nombre(s.quantiteActuelle, 2)} {s.uniteMesureLibelle}
                  </Badge>
                </li>
              ))}
            </ul>
          )}
        </Carte>

        {voitVentes && (
          <Carte
            className="apparition"
            titre="Dernières ventes"
            action={
              <Link to="/ventes" className="lien-action">
                Ventes
              </Link>
            }
          >
            {ventes.length === 0 ? (
              <p className="discret">Aucune vente enregistrée.</p>
            ) : (
              <ul className="liste-compacte">
                {[...ventes]
                  .sort((a, b) => b.dateVente.localeCompare(a.dateVente))
                  .slice(0, 5)
                  .map((v) => (
                    <li key={v.id}>
                      <span className="liste-compacte-texte">
                        <strong>{v.clientNom}</strong>
                        <span className="discret"> · {dateLisible(v.dateVente)}</span>
                      </span>
                      <strong className={v.estPaye ? '' : 'texte-danger'}>{monnaie(v.montantTotal)}</strong>
                    </li>
                  ))}
              </ul>
            )}
          </Carte>
        )}

        <Carte className="apparition carte-aide" titre="Raccourcis">
          <div className="raccourcis">
            <Link to="/bandes?statut=EnCours" className="raccourci">
              <IconePoussin taille={22} /> Bandes en cours
            </Link>
            <Link to="/stocks?etat=perimes" className="raccourci">
              <IconeCarton taille={22} /> Stocks périmés
            </Link>
            <Link to="/alertes" className="raccourci">
              <IconeCloche taille={22} /> Toutes les alertes
            </Link>
          </div>
        </Carte>
      </div>
    </main>
  )
}
