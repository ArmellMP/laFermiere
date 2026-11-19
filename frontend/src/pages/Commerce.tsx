// Écrans du module Commerce : Ventes, Achats, Clients, Fournisseurs.
// Même structure partout : en-tête, indicateurs, filtres, tableau triable.
import { useMemo, useState } from 'react'
import { api } from '../api/client'
import { useAuth } from '../auth/AuthContext'
import { useNotifier } from '../components/Formulaire'
import { FormulaireAchat, FormulaireClient, FormulaireFournisseur, FormulaireVente } from '../formulaires/Formulaires'
import { avecNomsAchats, avecNomsVentes } from '../api/noms'
import type { Achat, Client, Fournisseur, Vente } from '../api/types'
import { colonnesAchat, colonnesVente } from '../colonnes'
import { EtatChargement, useDonnees } from '../components/Chargement'
import { IconeAlerte, IconeCamion, IconePanier, IconePersonnes, IconePlus, IconeRecherche, IconeVente } from '../components/Icones'
import { Badge, BarreOutils, EnTetePage, Indicateurs, Puces, Recherche, Vide, useEtatUrl } from '../components/Page'
import { Tableau, type Colonne } from '../components/Tableau'
import { TableauAchats, TableauVentes } from '../formulaires/Gestion'
import { aujourdhui, monnaie, monnaieCompacte, nombre, normaliser } from '../outils'

const moisCourant = () => aujourdhui().slice(0, 7)

/** Bouton d'action principal de l'en-tête (« Nouvelle vente »…) */
function BoutonNouveau({ libelle, onClick }: { libelle: string; onClick: () => void }) {
  return (
    <button className="bouton-principal" onClick={onClick}>
      <IconePlus taille={20} /> {libelle}
    </button>
  )
}
const nomMois = () => new Date().toLocaleDateString('fr-FR', { month: 'long' })

// ---------------------------------------------------------------------------
// Ventes
// ---------------------------------------------------------------------------
export function Ventes() {
  const [paiement, setPaiement] = useEtatUrl<'toutes' | 'payees' | 'a-encaisser'>('paiement', 'toutes')
  const [recherche, setRecherche] = useState('')
  const [formulaire, setFormulaire] = useEtatUrl<'' | 'vente'>('formulaire', '')
  const { aLeRole } = useAuth()
  const notifier = useNotifier()
  const { donnees, erreur, enCours, recharger } = useDonnees(async () => avecNomsVentes(await api.liste<Vente>('/Ventes')), [])
  const ventes = useMemo(() => donnees ?? [], [donnees])

  const filtrees = useMemo(() => {
    const q = normaliser(recherche.trim())
    return ventes
      .filter((v) => (paiement === 'payees' ? v.estPaye : paiement === 'a-encaisser' ? !v.estPaye : true))
      .filter((v) => !q || normaliser(`${v.clientNom ?? ''} ${v.bandeCode ?? ''} ${v.typeVenteLibelle}`).includes(q))
  }, [ventes, paiement, recherche])

  const total = ventes.reduce((s, v) => s + v.montantTotal, 0)
  const duMois = ventes.filter((v) => v.dateVente.startsWith(moisCourant())).reduce((s, v) => s + v.montantTotal, 0)
  const aEncaisser = ventes.filter((v) => !v.estPaye)
  const montantAEncaisser = aEncaisser.reduce((s, v) => s + v.montantTotal, 0)

  return (
    <main className="page">
      <EnTetePage miettes={[{ libelle: 'Tableau de bord', vers: '/' }, { libelle: 'Commerce' }, { libelle: 'Ventes' }]} titre="Ventes" sousTitre="Poulets vivants, nettoyés, fumés et fientes vendus."
        actions={aLeRole(...(['Admin', 'Comptable'] as const)) && <BoutonNouveau libelle="Nouvelle vente" onClick={() => setFormulaire('vente')} />}
      />
      <FormulaireVente
        ouvert={formulaire === 'vente'}
        onFermer={() => setFormulaire('')}
        onCree={(x) => {
          setFormulaire('')
          notifier(((v) => `Vente de ${monnaie(v.montantTotal)} enregistrée`)(x))
          void recharger()
        }}
      />
      <Indicateurs
        elements={[
          { libelle: 'Chiffre d’affaires', valeur: total, format: monnaieCompacte, icone: <IconeVente taille={20} />, ton: 'vert', detail: `${ventes.length} vente${ventes.length > 1 ? 's' : ''}` },
          { libelle: `Ventes de ${nomMois()}`, valeur: duMois, format: monnaieCompacte, icone: <IconeVente taille={20} />, ton: 'ciel', detail: 'mois en cours' },
          {
            libelle: 'À encaisser',
            valeur: montantAEncaisser,
            format: monnaieCompacte,
            icone: <IconeAlerte taille={20} />,
            ton: aEncaisser.length ? 'danger' : 'neutre',
            detail: `${aEncaisser.length} vente${aEncaisser.length > 1 ? 's' : ''} non payée${aEncaisser.length > 1 ? 's' : ''}`,
          },
        ]}
      />
      <BarreOutils>
        <Puces
          libelle="Filtrer par paiement"
          valeur={paiement}
          onChange={setPaiement}
          options={[
            { valeur: 'toutes', libelle: 'Toutes', compte: ventes.length },
            { valeur: 'payees', libelle: 'Payées', compte: ventes.length - aEncaisser.length, ton: 'vert' },
            { valeur: 'a-encaisser', libelle: 'À encaisser', compte: aEncaisser.length, ton: 'danger' },
          ]}
        />
        <div className="barre-outils-ligne">
          <Recherche valeur={recherche} onChange={setRecherche} placeholder="Client, bande, produit…" />
        </div>
      </BarreOutils>
      <EtatChargement enCours={enCours} erreur={erreur} recharger={recharger} squelettes={3} />
      {!enCours && !erreur && (
        <>
          <p className="resultat-compte">
            {filtrees.length} vente{filtrees.length > 1 ? 's' : ''} · <strong>{monnaie(filtrees.reduce((s, v) => s + v.montantTotal, 0))}</strong>
          </p>
          <TableauVentes
            lignes={filtrees}
            colonnes={colonnesVente}
            lien={(v) => `/bandes/${v.bandeId}?onglet=ventes`}
            vide={<Vide icone={<IconeVente taille={28} />} titre="Aucune vente" texte="Aucune vente ne correspond à ces filtres." />}
            onChange={() => void recharger()}
          />
        </>
      )}
    </main>
  )
}

// ---------------------------------------------------------------------------
// Achats
// ---------------------------------------------------------------------------
export function Achats() {
  const [reception, setReception] = useEtatUrl<'tous' | 'recus' | 'attente'>('reception', 'tous')
  const [recherche, setRecherche] = useState('')
  const [formulaire, setFormulaire] = useEtatUrl<'' | 'achat'>('formulaire', '')
  const { aLeRole } = useAuth()
  const notifier = useNotifier()
  const { donnees, erreur, enCours, recharger } = useDonnees(async () => avecNomsAchats(await api.liste<Achat>('/Achats')), [])
  const achats = useMemo(() => donnees ?? [], [donnees])

  const filtres = useMemo(() => {
    const q = normaliser(recherche.trim())
    return achats
      .filter((a) => (reception === 'recus' ? a.estRecu : reception === 'attente' ? !a.estRecu : true))
      .filter((a) => !q || normaliser(`${a.fournisseurNom ?? ''} ${a.bandeCode ?? ''} ${a.typeAchatLibelle} ${a.factureNumero ?? ''}`).includes(q))
  }, [achats, reception, recherche])

  const total = achats.reduce((s, a) => s + a.montantTotal, 0)
  const duMois = achats.filter((a) => a.dateAchat.startsWith(moisCourant())).reduce((s, a) => s + a.montantTotal, 0)
  const enAttente = achats.filter((a) => !a.estRecu).length

  return (
    <main className="page">
      <EnTetePage miettes={[{ libelle: 'Tableau de bord', vers: '/' }, { libelle: 'Commerce' }, { libelle: 'Achats' }]} titre="Achats" sousTitre="Poussins, aliment, médicaments, matériel et services."
        actions={aLeRole(...(['Admin', 'Comptable'] as const)) && <BoutonNouveau libelle="Nouvel achat" onClick={() => setFormulaire('achat')} />}
      />
      <FormulaireAchat
        ouvert={formulaire === 'achat'}
        onFermer={() => setFormulaire('')}
        onCree={(x) => {
          setFormulaire('')
          notifier(((a) => `Achat de ${monnaie(a.montantTotal)} enregistré`)(x))
          void recharger()
        }}
      />
      <Indicateurs
        elements={[
          { libelle: 'Total des achats', valeur: total, format: monnaieCompacte, icone: <IconePanier taille={20} />, ton: 'terre', detail: `${achats.length} achat${achats.length > 1 ? 's' : ''}` },
          { libelle: `Achats de ${nomMois()}`, valeur: duMois, format: monnaieCompacte, icone: <IconePanier taille={20} />, ton: 'ciel', detail: 'mois en cours' },
          { libelle: 'En attente de réception', valeur: enAttente, icone: <IconeCamion taille={20} />, ton: enAttente ? 'soleil' : 'neutre', detail: 'livraisons à venir' },
        ]}
      />
      <BarreOutils>
        <Puces
          libelle="Filtrer par réception"
          valeur={reception}
          onChange={setReception}
          options={[
            { valeur: 'tous', libelle: 'Tous', compte: achats.length },
            { valeur: 'recus', libelle: 'Reçus', compte: achats.length - enAttente, ton: 'vert' },
            { valeur: 'attente', libelle: 'En attente', compte: enAttente, ton: 'soleil' },
          ]}
        />
        <div className="barre-outils-ligne">
          <Recherche valeur={recherche} onChange={setRecherche} placeholder="Fournisseur, bande, facture…" />
        </div>
      </BarreOutils>
      <EtatChargement enCours={enCours} erreur={erreur} recharger={recharger} squelettes={3} />
      {!enCours && !erreur && (
        <>
          <p className="resultat-compte">
            {filtres.length} achat{filtres.length > 1 ? 's' : ''} · <strong>{monnaie(filtres.reduce((s, a) => s + a.montantTotal, 0))}</strong>
          </p>
          <TableauAchats
            lignes={filtres}
            colonnes={colonnesAchat}
            lien={(a) => `/bandes/${a.bandeId}?onglet=achats`}
            vide={<Vide icone={<IconePanier taille={28} />} titre="Aucun achat" texte="Aucun achat ne correspond à ces filtres." />}
            onChange={() => void recharger()}
          />
        </>
      )}
    </main>
  )
}

// ---------------------------------------------------------------------------
// Clients et fournisseurs : annuaires
// ---------------------------------------------------------------------------
function Contact({ telephone, email }: { telephone?: string | null; email?: string | null }) {
  if (!telephone && !email) return <span className="discret">—</span>
  return (
    <span className="contact">
      {telephone && (
        <a href={`tel:${telephone}`} onClick={(e) => e.stopPropagation()}>
          {telephone}
        </a>
      )}
      {email && <span className="discret">{email}</span>}
    </span>
  )
}

export function Clients() {
  const [type, setType] = useEtatUrl<string>('type', 'tous')
  const [recherche, setRecherche] = useState('')
  const [formulaire, setFormulaire] = useEtatUrl<'' | 'client'>('formulaire', '')
  const { aLeRole } = useAuth()
  const notifier = useNotifier()
  const { donnees, erreur, enCours, recharger } = useDonnees(() => api.liste<Client>('/Clients'), [])
  const clients = useMemo(() => donnees ?? [], [donnees])
  const types = [...new Map(clients.map((c) => [c.typeClient, c.typeClientLibelle])).entries()]

  const filtres = clients
    .filter((c) => type === 'tous' || c.typeClient === type)
    .filter((c) => !recherche.trim() || normaliser(`${c.nom} ${c.telephone ?? ''} ${c.email ?? ''}`).includes(normaliser(recherche.trim())))

  const colonnes: Colonne<Client>[] = [
    { cle: 'nom', titre: 'Client', rendu: (c) => <strong>{c.nom}</strong>, tri: (c) => c.nom, mobile: 'titre' },
    { cle: 'type', titre: 'Type', rendu: (c) => <Badge ton="ciel">{c.typeClientLibelle}</Badge>, tri: (c) => c.typeClientLibelle, mobile: 'sous-titre' },
    { cle: 'contact', titre: 'Contact', rendu: (c) => <Contact telephone={c.telephone} email={c.email} />, mobile: 'detail' },
    { cle: 'ventes', titre: 'Ventes', rendu: (c) => nombre(c.nombreVentes ?? 0), tri: (c) => c.nombreVentes ?? 0, alignement: 'droite', mobile: 'detail' },
    { cle: 'montant', titre: 'Total acheté', rendu: (c) => <strong>{monnaie(c.montantTotalAchats ?? 0)}</strong>, tri: (c) => c.montantTotalAchats ?? 0, alignement: 'droite', mobile: 'droite' },
    { cle: 'actif', titre: 'Statut', rendu: (c) => (c.estActif ? <Badge ton="vert">Actif</Badge> : <Badge>Inactif</Badge>), tri: (c) => Number(c.estActif), mobile: 'masquer' },
  ]

  return (
    <main className="page">
      <EnTetePage miettes={[{ libelle: 'Tableau de bord', vers: '/' }, { libelle: 'Commerce' }, { libelle: 'Clients' }]} titre="Clients" sousTitre="Particuliers, restaurants, marchés et grossistes."
        actions={aLeRole(...(['Admin', 'Comptable'] as const)) && <BoutonNouveau libelle="Nouveau client" onClick={() => setFormulaire('client')} />}
      />
      <FormulaireClient
        ouvert={formulaire === 'client'}
        onFermer={() => setFormulaire('')}
        onCree={(x) => {
          setFormulaire('')
          notifier(((c) => `Client ${c.nom} ajouté`)(x))
          void recharger()
        }}
      />
      <Indicateurs
        elements={[
          { libelle: 'Clients', valeur: clients.length, icone: <IconePersonnes taille={20} />, ton: 'ciel', detail: `${clients.filter((c) => c.estActif).length} actifs` },
          {
            libelle: 'Meilleur client',
            valeur: null,
            detail: (() => {
              const meilleur = [...clients].sort((a, b) => (b.montantTotalAchats ?? 0) - (a.montantTotalAchats ?? 0))[0]
              return meilleur && (meilleur.montantTotalAchats ?? 0) > 0 ? `${meilleur.nom} · ${monnaie(meilleur.montantTotalAchats ?? 0)}` : 'aucune vente'
            })(),
            icone: <IconeVente taille={20} />,
            ton: 'vert',
          },
        ]}
      />
      <BarreOutils>
        <Puces
          libelle="Filtrer par type"
          valeur={type}
          onChange={setType}
          options={[{ valeur: 'tous', libelle: 'Tous', compte: clients.length }, ...types.map(([v, l]) => ({ valeur: v, libelle: l, compte: clients.filter((c) => c.typeClient === v).length }))]}
        />
        <div className="barre-outils-ligne">
          <Recherche valeur={recherche} onChange={setRecherche} placeholder="Nom, téléphone, e-mail…" />
        </div>
      </BarreOutils>
      <EtatChargement enCours={enCours} erreur={erreur} recharger={recharger} squelettes={3} />
      {!enCours && !erreur && (
        <Tableau
          lignes={filtres}
          colonnes={colonnes}
          cle={(c) => c.id}
          triInitial={{ cle: 'montant', sens: 'desc' }}
          vide={<Vide icone={<IconeRecherche taille={28} />} titre="Aucun client" texte="Aucun client ne correspond à ces filtres." />}
        />
      )}
    </main>
  )
}

export function Fournisseurs() {
  const [type, setType] = useEtatUrl<string>('type', 'tous')
  const [recherche, setRecherche] = useState('')
  const [formulaire, setFormulaire] = useEtatUrl<'' | 'fournisseur'>('formulaire', '')
  const { aLeRole } = useAuth()
  const notifier = useNotifier()
  const { donnees, erreur, enCours, recharger } = useDonnees(() => api.liste<Fournisseur>('/Fournisseurs'), [])
  const fournisseurs = useMemo(() => donnees ?? [], [donnees])
  const types = [...new Map(fournisseurs.map((f) => [f.typeFournisseur, f.typeFournisseurLibelle])).entries()]

  const filtres = fournisseurs
    .filter((f) => type === 'tous' || f.typeFournisseur === type)
    .filter((f) => !recherche.trim() || normaliser(`${f.nom} ${f.contactPersonne ?? ''} ${f.telephone ?? ''}`).includes(normaliser(recherche.trim())))

  const colonnes: Colonne<Fournisseur>[] = [
    { cle: 'nom', titre: 'Fournisseur', rendu: (f) => <strong>{f.nom}</strong>, tri: (f) => f.nom, mobile: 'titre' },
    { cle: 'type', titre: 'Spécialité', rendu: (f) => <Badge ton="terre">{f.typeFournisseurLibelle}</Badge>, tri: (f) => f.typeFournisseurLibelle, mobile: 'sous-titre' },
    { cle: 'personne', titre: 'Interlocuteur', rendu: (f) => f.contactPersonne ?? <span className="discret">—</span>, tri: (f) => f.contactPersonne ?? '', mobile: 'detail' },
    { cle: 'contact', titre: 'Contact', rendu: (f) => <Contact telephone={f.telephone} email={f.email} />, mobile: 'detail' },
    { cle: 'achats', titre: 'Achats', rendu: (f) => nombre(f.nombreAchats ?? 0), tri: (f) => f.nombreAchats ?? 0, alignement: 'droite', mobile: 'detail' },
    { cle: 'montant', titre: 'Total payé', rendu: (f) => <strong>{monnaie(f.montantTotalAchats ?? 0)}</strong>, tri: (f) => f.montantTotalAchats ?? 0, alignement: 'droite', mobile: 'droite' },
  ]

  return (
    <main className="page">
      <EnTetePage miettes={[{ libelle: 'Tableau de bord', vers: '/' }, { libelle: 'Commerce' }, { libelle: 'Fournisseurs' }]} titre="Fournisseurs" sousTitre="Poussinières, fabricants d’aliment, vétérinaires et transporteurs."
        actions={aLeRole(...(['Admin', 'Comptable'] as const)) && <BoutonNouveau libelle="Nouveau fournisseur" onClick={() => setFormulaire('fournisseur')} />}
      />
      <FormulaireFournisseur
        ouvert={formulaire === 'fournisseur'}
        onFermer={() => setFormulaire('')}
        onCree={(x) => {
          setFormulaire('')
          notifier(((f) => `Fournisseur ${f.nom} ajouté`)(x))
          void recharger()
        }}
      />
      <Indicateurs
        elements={[
          { libelle: 'Fournisseurs', valeur: fournisseurs.length, icone: <IconeCamion taille={20} />, ton: 'terre', detail: `${fournisseurs.filter((f) => f.estActif).length} actifs` },
          {
            libelle: 'Total payé',
            valeur: fournisseurs.reduce((s, f) => s + (f.montantTotalAchats ?? 0), 0),
            format: monnaieCompacte,
            icone: <IconePanier taille={20} />,
            ton: 'ciel',
            detail: 'tous fournisseurs',
          },
        ]}
      />
      <BarreOutils>
        <Puces
          libelle="Filtrer par spécialité"
          valeur={type}
          onChange={setType}
          options={[
            { valeur: 'tous', libelle: 'Tous', compte: fournisseurs.length },
            ...types.map(([v, l]) => ({ valeur: v, libelle: l, compte: fournisseurs.filter((f) => f.typeFournisseur === v).length })),
          ]}
        />
        <div className="barre-outils-ligne">
          <Recherche valeur={recherche} onChange={setRecherche} placeholder="Nom, interlocuteur, téléphone…" />
        </div>
      </BarreOutils>
      <EtatChargement enCours={enCours} erreur={erreur} recharger={recharger} squelettes={3} />
      {!enCours && !erreur && (
        <Tableau
          lignes={filtres}
          colonnes={colonnes}
          cle={(f) => f.id}
          triInitial={{ cle: 'montant', sens: 'desc' }}
          vide={<Vide icone={<IconeRecherche taille={28} />} titre="Aucun fournisseur" texte="Aucun fournisseur ne correspond à ces filtres." />}
        />
      )}
    </main>
  )
}
