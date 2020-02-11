using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.Modeling;

namespace windwaker_include_exclude_activities
{    
    public static class activity_selector
    {        
        static MapContainer<string, bool> activity_included = new MapContainer<string, bool>();
        static SetContainer<string> activity_selected = new SetContainer<string>();
        public static SetContainer<Activity> activities = new SetContainer<Activity>();
        static MapContainer<string, SetContainer<Document>> documents = new MapContainer<string, SetContainer<Document>>();
        static string doc1 = "Doc1";
        static int i;
        static doc_selection_status new_doc_selection_status;
        static bool dialog_status_flag;
        static SetContainer<Document> result;

        public static Set<string> IncludedActivityNames()
        {
            return new Set<string>(activity_included.Keys);
        }

        public static SetContainer<Activity> ActivityList()
        {
            return new SetContainer<Activity>(activities);        
        }

        public static SetContainer<Document> DocumentList()
        {
             result = new SetContainer<Document>();

            foreach (Activity act1 in activities)
                if (activity_selected.Contains(act1.name))
                {
                    result = documents[act1.name];
                    return new SetContainer<Document>(result.ToArray());
                }

            return new SetContainer<Document>();
            
        }

        [Rule]
        public static void include_activity(Activity act)
        {
            Requires(!activity_included.Keys.Contains(act.name));
            activity_included.Add(act.name, true);
            activities.Add(act);
            BuildActivityDocumentsList(act);
        }

        [Rule]
        public static void select_activity([Domain("IncludedActivityNames")] string act_name)
        {
            Requires(activity_included.Keys.Contains(act_name));
            Requires(!activity_selected.Contains(act_name));
            activity_selected.Add(act_name);
        }
        
        [Rule]
        public static void change_document_selection([Domain("ActivityList")] Activity act, [Domain("DocumentList")] Document doc)
        {
            Requires(activity_selected.Contains(act.name));
            documents[act.name] = ChangeDocumentSelectionStatus(doc.name, documents[act.name]);
            dialog_status_flag = true;

        }

        [Rule]
        public static void dialog_status([Domain("ActivityList")] Activity act, [Domain("DocumentList")] Document doc)
        {
            Requires(dialog_status_flag);
            dialog_status_flag = false;

        }

        static doc_selection_status GetDocumentSelectionStatus(string doc_name, SetContainer<Document> docs)
        {
            foreach (Document d in docs)
            {
                if (doc_name == d.name)
                {                    
                    return d.status;
                }                                
            }
            return doc_selection_status.null_none;
        }

        static SetContainer<Document> ChangeDocumentSelectionStatus(string name, SetContainer<Document> docs)
        {
            SetContainer<Document> new_docs = new SetContainer<Document>();
            doc_selection_status current_value = GetDocumentSelectionStatus(name, docs);
            i = (int)current_value;
            if (i < 2)
            {
                i++;                
            }
            else
            {
                i = 0;
            }

            new_doc_selection_status = (doc_selection_status)i;

            foreach (Document d in docs)
            {
                if (name == d.name)
                {
                    new_docs.Add(new Document(d.name, new_doc_selection_status, d.type));
                }                
            }

            return new_docs;
        }

        static void Requires(bool condition)
        {
            Condition.IsTrue(condition);
        }

        static void BuildActivityDocumentsList(Activity act)
        {
            new_doc_selection_status = doc_selection_status.included;
            if (act.isMandatory && !documents.Keys.Contains(act.name))
            {
                documents.Add(new KeyValuePair<string, SetContainer<Document>>(act.name, new SetContainer<Document>
                    (new Document(doc1, new_doc_selection_status, document_type.mandatory))));            
            }
        
        }
        

    }

    public struct Activity
    {
        public string name;
        public bool isMandatory;

        public Activity(string name, bool isMandatory)
        {
            this.name = name;
            this.isMandatory = isMandatory;            
        }
    
    }

    public struct Document
    {
        public string name;
        public doc_selection_status status;
        public document_type type;

        public Document(string name, doc_selection_status status, document_type type)
        {
            this.name = name;
            this.status = status;
            this.type = type;
        }
    
    }

    public enum doc_selection_status
    {
        null_none = 0,
        included = 1,
        excluded = 2
    }

    public enum document_type
    {
        mandatory,
        optional    
    }

}

namespace windwaker_inc_exc_part2
{
    public static class Parameters
    {
        public static int max_number_of_selection_changes = 0;
        public static SetContainer<string> set;
        public static Activity activity_one;
        public static SetContainer<Activity> acts;
        public static bool login_success = false;
        public static bool file_directory_status = false;
        public static file_directory fd = file_directory.none;
    }

    public enum file_directory
    { 
        none,
        create,
        edit
    
    }
    
    [Feature("Login")]
    public static class login
    {       
       [Rule] 
       public static void login_success()
       {
           Condition.IsTrue(!Parameters.login_success);
           Parameters.login_success = true;
           Parameters.fd = file_directory.none;
       }
    }

    [Feature("CreateOrEdit")]
    public static class create_or_edit
    {
        [Rule]
        public static void create_new_plan()
        {
            Condition.IsTrue(Parameters.login_success);
            Condition.IsTrue(!Parameters.file_directory_status);
            Condition.IsTrue(Parameters.fd != file_directory.edit);
            Condition.IsTrue(include_exclude.activity_selected.Count == 0);
            Condition.IsTrue(include_exclude.activities.Count == 0);
            Parameters.fd = file_directory.create;
            Parameters.file_directory_status = true;        
        }

        //[Rule]
        //public static void edit_existing_plan1()
        //{
        //    Condition.IsTrue(Parameters.login_success);
        //    Condition.IsTrue(!Parameters.file_directory_status);
        //    Condition.IsTrue(Parameters.fd != file_directory.create);
        //    Parameters.fd = file_directory.edit;
        //    Parameters.file_directory_status = true;            
        //}

        //[Rule]
        //public static void load_plan()
        //{
        //    Condition.IsTrue(Parameters.fd == file_directory.create || Parameters.fd == file_directory.edit);
        //}
        
    }


    [Feature("IncludeExclude")]
    public static class include_exclude
    {
        static MapContainer<string, bool> activity_included = new MapContainer<string, bool>();
        static MapContainer<string, bool> activity_excluded = new MapContainer<string, bool>();
        public static SetContainer<Activity> activity_selected = new SetContainer<Activity>();
        public static SetContainer<Activity> activities = new SetContainer<Activity>();
        static MapContainer<Activity, SetContainer<Document>> documents = new MapContainer<Activity, SetContainer<Document>>();
        static string doc1 = "Doc1";
        static int i;
        static doc_selection_status new_doc_selection_status;
        static activity_selection_status new_activity_selection_status;
        static bool dialog_status_flag;
        static bool plan_view_loaded_flag;
        static SetContainer<Document> result;
        public enum gui_status { select_activity , flyout_loaded};
        static gui_status guistatus = gui_status.select_activity;
        static bool flyout_loaded = false;        
        static int selection_changed_counter;

        public static Set<string> IncludedActivityNames()
        {
            return new Set<string>(activity_included.Keys);
        }

        public static SetContainer<Activity> SelectedActivityNames()
        {
            return new SetContainer<Activity>(activity_selected);
        }

        public static SetContainer<Activity> ActivityList()
        {
            return new SetContainer<Activity>(activities);
        }

        public static SetContainer<Document> DocumentList()
        {
            result = new SetContainer<Document>();

            foreach (Activity act1 in activities)
                if (activity_selected.Contains(act1))
                {
                    result = documents[act1];
                    return new SetContainer<Document>(result.ToArray());
                }

            return new SetContainer<Document>();

        }

        [Rule]
        public static void edit_existing_plan()
        {
            Condition.IsTrue(Parameters.login_success);
            Condition.IsTrue(!Parameters.file_directory_status);            
            Requires(activity_selected.Count == 0);
            Requires(activities.Count == 0);
            Requires(!plan_view_loaded_flag);
            plan_view_loaded_flag = true;
            activities = Parameters.acts;
        }

        //[Rule]
        //public static void plan_loaded(Activity act)
        //{
        //    Requires(!activities.Contains(act));
        //    Requires(plan_view_loaded_flag == true);
        //    activities.Add(act);
        //}

        [Rule]
        public static void select_activity([Domain("ActivityList")] Activity act)
        {
            Requires(!activity_selected.Contains(act));
            Requires(activities.Count > 0);
            changeSelection(act);
            flyout_loaded = true;
        }

        [Rule]
        public static void change_activity_selection([Domain("SelectedActivityNames")] Activity selected_acts, activity_selection_status ass)
        {
            Requires(selection_changed_counter < Parameters.max_number_of_selection_changes);
            Activity activity_changed;
            activities = ChangeActivitySelectionStatus(selected_acts, ass, out activity_changed);
            activity_selected = new SetContainer<Activity>();
            activity_selected.Add(activity_changed);
            selection_changed_counter++;
        }

        [Rule]
        public static void close_flyout([Domain("SelectedActivityNames")] Activity selected_acts)
        {
            Requires(flyout_loaded);
            activity_selected = new SetContainer<Activity>();

            if (selection_changed_counter == Parameters.max_number_of_selection_changes)
            {
                activities = new SetContainer<Activity>();
            }
        }

        [AcceptingStateCondition]
        public static bool accept_close_flyout()
        {
            return (activity_selected.Count == 0) && (activities.Count == 0);
        }

        public static void changeSelection(Activity newSelection)
        {
            if (activity_selected.Count > 0)
            {
                activity_selected = new SetContainer<Activity>();                
            }
            activity_selected.Add(newSelection);
        }

        //[Rule]
        //public static void include_activity([Domain("ActivityList")] Activity act)
        //{
        //    Requires(activities.Contains(act));
        //    activities = ChangeActivitySelectionStatus(act);
        //    BuildActivityDocumentsList(act, doc_selection_status.included);
        //}

        //[Rule]
        //public static void exclude_activity([Domain("ActivityList")] Activity act)
        //{
        //    Requires(activities.Contains(act));
        //    activities = ChangeActivitySelectionStatus(act, activity_selection_status.excluded);
        //    BuildActivityDocumentsList(act, doc_selection_status.excluded);
        //}


        //[Rule]
        //public static void change_document_selection([Domain("ActivityList")] Activity act, [Domain("DocumentList")] Document doc)
        //{
        //    Requires(activity_selected.Contains(act));
        //    documents[act] = ChangeDocumentSelectionStatus(doc.name, documents[act]);
        //    dialog_status_flag = true;

        //}

        //[Rule]
        //public static void dialog_status([Domain("ActivityList")] Activity act, [Domain("DocumentList")] Document doc)
        //{
        //    Requires(dialog_status_flag);
        //    dialog_status_flag = false;

        //}

        static void Requires(bool condition)
        {
            Condition.IsTrue(condition);
        }

        static doc_selection_status GetDocumentSelectionStatus(string doc_name, SetContainer<Document> docs)
        {
            foreach (Document d in docs)
            {
                if (doc_name == d.name)
                {
                    return d.status;
                }
            }
            return doc_selection_status.null_none;
        }

        static SetContainer<Document> ChangeDocumentSelectionStatus(string name, SetContainer<Document> docs)
        {
            SetContainer<Document> new_docs = new SetContainer<Document>();
            doc_selection_status current_value = GetDocumentSelectionStatus(name, docs);
            i = (int)current_value;
            if (i < 2)
            {
                i++;
            }
            else
            {
                i = 0;
            }

            new_doc_selection_status = (doc_selection_status)i;

            foreach (Document d in docs)
            {
                if (name == d.name)
                {
                    new_docs.Add(new Document(d.name, new_doc_selection_status, d.type));
                }
            }

            return new_docs;
        }       

        //private static SetContainer<Activity> ChangeActivitySelectionStatus(Activity act)
        //{
        //    activity_selection_status current_value = act.status;
        //    i = (int)current_value;
        //    if (i < 3)
        //    {
        //        i++;
        //    }
        //    else
        //    {
        //        i = 0;
        //    }

        //    new_activity_selection_status = (activity_selection_status)i;

        //    return ChangeActivitySelectionStatus(act, new_activity_selection_status, out act_changed);
        //}

        private static SetContainer<Activity> ChangeActivitySelectionStatus(Activity act, activity_selection_status ass, out Activity status_changed_act)
        {
            SetContainer<Activity> new_activities = new SetContainer<Activity>();
            status_changed_act = new Activity(act.name, ass, act.isMandatory);

            foreach (Activity a in activities)
            {
                if (act.name == a.name)
                {                    
                    new_activities.Add(status_changed_act);
                }
            }

            return new_activities;
        }

        static void BuildActivityDocumentsList(Activity act, doc_selection_status new_doc_selection_status)
        {
            if (act.isMandatory && !documents.Keys.Contains(act))
            {
                documents.Add(new KeyValuePair<Activity, SetContainer<Document>>(act, new SetContainer<Document>
                    (new Document(doc1, new_doc_selection_status, document_type.mandatory))));
            }

        }


    }

    public struct Activity
    {
        public string name;
        public activity_selection_status status;
        public bool isMandatory;

        public Activity(string name, activity_selection_status status, bool isMandatory)
        {
            this.name = name;
            this.status = status;
            this.isMandatory = isMandatory;
        }

    }

    public struct Document
    {
        public string name;
        public doc_selection_status status;
        public document_type type;

        public Document(string name, doc_selection_status status, document_type type)
        {
            this.name = name;
            this.status = status;
            this.type = type;
        }

    }

    public enum doc_selection_status
    {
        null_none = 0,
        included = 1,
        excluded = 2
    }

    public enum activity_selection_status
    {
        no_selection = 0,
        out_of_scope = 1,
        include = 2,
        exclude = 3
    }

    public enum document_type
    {
        mandatory,
        optional
    }

}

namespace windwaker_inc_out_of_scope
{
    public static class activity_selector
    {
        static MapContainer<string, bool> activity_included = new MapContainer<string, bool>();
        static MapContainer<string, bool> activity_excluded = new MapContainer<string, bool>();
        static SetContainer<Activity> activity_selected = new SetContainer<Activity>();
        public static SetContainer<Activity> activities = new SetContainer<Activity>();
        static MapContainer<Activity, SetContainer<Document>> documents = new MapContainer<Activity, SetContainer<Document>>();
        static string doc1 = "Doc1";
        static int i;
        static doc_selection_status new_doc_selection_status;
        static activity_selection_status new_activity_selection_status;
        static bool dialog_status_flag;
        static SetContainer<Document> result;
        public enum gui_status { none, select_activity, flyout_loaded };
        static gui_status guistatus = gui_status.none;
        static int selection_changed_counter = 0;
        static int max_number_of_selection_changes = 0;

        public static Set<string> IncludedActivityNames()
        {
            return new Set<string>(activity_included.Keys);
        }

        public static SetContainer<Activity> SelectedActivityNames()
        {
            return new SetContainer<Activity>(activity_selected);
        }

        public static SetContainer<Activity> ActivityList()
        {
            return new SetContainer<Activity>(activities);
        }

        public static SetContainer<Document> DocumentList()
        {
            result = new SetContainer<Document>();

            foreach (Activity act1 in activities)
                if (activity_selected.Contains(act1))
                {
                    result = documents[act1];
                    return new SetContainer<Document>(result.ToArray());
                }

            return new SetContainer<Document>();

        }

        [Rule]
        public static void edit_existing_plan(Activity act, int selection_changes)
        {
            Requires(!activities.Contains(act));
            Requires(guistatus == gui_status.none);
            activities.Add(act);
            guistatus = gui_status.select_activity;
            max_number_of_selection_changes = selection_changes;
        }

        [Rule]
        public static void select_activity([Domain("ActivityList")] Activity act)
        {
            Requires(activities.Contains(act));
            Requires(!activity_selected.Contains(act));
            Requires(guistatus == gui_status.select_activity);
            activity_selected.Add(act);
            guistatus = gui_status.flyout_loaded;
        }

        [Rule]
        public static void change_activity_selection([Domain("SelectedActivityNames")] Activity selected_acts, activity_selection_status ass)
        {
            Requires(guistatus == gui_status.flyout_loaded);
            Requires(selection_changed_counter < max_number_of_selection_changes);
            Activity activity_changed;
            activities = ChangeActivitySelectionStatus(selected_acts, ass, out activity_changed);
            activity_selected = new SetContainer<Activity>();
            activity_selected.Add(activity_changed);
            selection_changed_counter++;
        }

        [Rule]
        public static void close_flyout()
        {
            Requires(guistatus == gui_status.flyout_loaded);
            activity_selected = new SetContainer<Activity>();
            guistatus = gui_status.select_activity;

            if (selection_changed_counter == max_number_of_selection_changes)
            {
                activities = new SetContainer<Activity>();
            }
        }

        //[AcceptingStateCondition]
        //public static bool finish()
        //{
        //    return (selection_changed_counter < 3) && (activity_selected.Count == 0);
        //}

        //[Rule]
        //public static void include_activity([Domain("ActivityList")] Activity act)
        //{
        //    Requires(activities.Contains(act));
        //    activities = ChangeActivitySelectionStatus(act);
        //    BuildActivityDocumentsList(act, doc_selection_status.included);
        //}

        //[Rule]
        //public static void exclude_activity([Domain("ActivityList")] Activity act)
        //{
        //    Requires(activities.Contains(act));
        //    activities = ChangeActivitySelectionStatus(act, activity_selection_status.excluded);
        //    BuildActivityDocumentsList(act, doc_selection_status.excluded);
        //}


        //[Rule]
        //public static void change_document_selection([Domain("ActivityList")] Activity act, [Domain("DocumentList")] Document doc)
        //{
        //    Requires(activity_selected.Contains(act));
        //    documents[act] = ChangeDocumentSelectionStatus(doc.name, documents[act]);
        //    dialog_status_flag = true;

        //}

        //[Rule]
        //public static void dialog_status([Domain("ActivityList")] Activity act, [Domain("DocumentList")] Document doc)
        //{
        //    Requires(dialog_status_flag);
        //    dialog_status_flag = false;

        //}

        static doc_selection_status GetDocumentSelectionStatus(string doc_name, SetContainer<Document> docs)
        {
            foreach (Document d in docs)
            {
                if (doc_name == d.name)
                {
                    return d.status;
                }
            }
            return doc_selection_status.null_none;
        }

        static SetContainer<Document> ChangeDocumentSelectionStatus(string name, SetContainer<Document> docs)
        {
            SetContainer<Document> new_docs = new SetContainer<Document>();
            doc_selection_status current_value = GetDocumentSelectionStatus(name, docs);
            i = (int)current_value;
            if (i < 2)
            {
                i++;
            }
            else
            {
                i = 0;
            }

            new_doc_selection_status = (doc_selection_status)i;

            foreach (Document d in docs)
            {
                if (name == d.name)
                {
                    new_docs.Add(new Document(d.name, new_doc_selection_status, d.type));
                }
            }

            return new_docs;
        }

        static void Requires(bool condition)
        {
            Condition.IsTrue(condition);
        }

        //private static SetContainer<Activity> ChangeActivitySelectionStatus(Activity act)
        //{
        //    activity_selection_status current_value = act.status;
        //    i = (int)current_value;
        //    if (i < 3)
        //    {
        //        i++;
        //    }
        //    else
        //    {
        //        i = 0;
        //    }

        //    new_activity_selection_status = (activity_selection_status)i;

        //    return ChangeActivitySelectionStatus(act, new_activity_selection_status, out act_changed);
        //}

        private static SetContainer<Activity> ChangeActivitySelectionStatus(Activity act, activity_selection_status ass, out Activity status_changed_act)
        {
            SetContainer<Activity> new_activities = new SetContainer<Activity>();
            status_changed_act = new Activity(act.name, ass, act.isMandatory);

            foreach (Activity a in activities)
            {
                if (act.name == a.name)
                {
                    new_activities.Add(status_changed_act);
                }
            }

            return new_activities;
        }

        static void BuildActivityDocumentsList(Activity act, doc_selection_status new_doc_selection_status)
        {
            if (act.isMandatory && !documents.Keys.Contains(act))
            {
                documents.Add(new KeyValuePair<Activity, SetContainer<Document>>(act, new SetContainer<Document>
                    (new Document(doc1, new_doc_selection_status, document_type.mandatory))));
            }

        }


    }

    public struct Activity
    {
        public string name;
        public activity_selection_status status;
        public bool isMandatory;

        public Activity(string name, activity_selection_status status, bool isMandatory)
        {
            this.name = name;
            this.status = status;
            this.isMandatory = isMandatory;
        }

    }

    public struct Document
    {
        public string name;
        public doc_selection_status status;
        public document_type type;

        public Document(string name, doc_selection_status status, document_type type)
        {
            this.name = name;
            this.status = status;
            this.type = type;
        }

    }

    public enum doc_selection_status
    {
        null_none = 0,
        included = 1,
        excluded = 2
    }

    public enum activity_selection_status
    {
        no_selection = 0,
        out_of_scope = 1,
        include = 2,
        exclude = 3
    }

    public enum document_type
    {
        mandatory,
        optional
    }

}

namespace windwaker_checkbox_only_iteration
{
    public static class Parameters
    {
        public static int max_number_of_selection_changes = 0;
        public static SetContainer<string> set;
        public static Activity1 activity_one;
        public static SetContainer<Activity1> acts;
        public static bool login_success = false;
        public static bool file_directory_status = false;
        public static file_directory fd = file_directory.none;
    }

    public enum file_directory
    {
        none,
        create,
        edit

    }

    [Feature("Login")]
    public static class login
    {
        [Rule]
        public static void login_success()
        {
            Condition.IsTrue(!Parameters.login_success);
            Parameters.login_success = true;
            Parameters.fd = file_directory.none;
        }
    }

    [Feature("CreateOrEdit")]
    public static class create_or_edit
    {
        [Rule]
        public static void create_new_plan()
        {
            Condition.IsTrue(Parameters.login_success);
            Condition.IsTrue(!Parameters.file_directory_status);
            Condition.IsTrue(Parameters.fd != file_directory.edit);
            Condition.IsTrue(checkbox_only.activity_selected.Count == 0);
            Condition.IsTrue(checkbox_only.activities.Count == 0);
            Parameters.fd = file_directory.create;
            Parameters.file_directory_status = true;
        }

        //[Rule]
        //public static void edit_existing_plan1()
        //{
        //    Condition.IsTrue(Parameters.login_success);
        //    Condition.IsTrue(!Parameters.file_directory_status);
        //    Condition.IsTrue(Parameters.fd != file_directory.create);
        //    Parameters.fd = file_directory.edit;
        //    Parameters.file_directory_status = true;            
        //}

        //[Rule]
        //public static void load_plan()
        //{
        //    Condition.IsTrue(Parameters.fd == file_directory.create || Parameters.fd == file_directory.edit);
        //}

    }


    [Feature("IncludeExclude")]
    public static class checkbox_only
    {
        static MapContainer<string, bool> activity_included = new MapContainer<string, bool>();
        static MapContainer<string, bool> activity_excluded = new MapContainer<string, bool>();
        public static SetContainer<Activity1> activity_selected = new SetContainer<Activity1>();
        public static SetContainer<Activity1> activities = new SetContainer<Activity1>();
        static MapContainer<Activity1, SetContainer<Document>> documents = new MapContainer<Activity1, SetContainer<Document>>();
        static string doc1 = "Doc1";
        static int i;
        static doc_selection_status new_doc_selection_status;
        static activity_selection_status new_activity_selection_status;
        static bool dialog_status_flag;
        static bool plan_view_loaded_flag;
        static SetContainer<Document> result;
        public enum gui_status { select_activity, flyout_loaded };
        static gui_status guistatus = gui_status.select_activity;
        static bool flyout_loaded = false;
        static int selection_changed_counter;
        public static SetContainer<string> activity_selection_status_set;

        public static Set<string> IncludedActivityNames()
        {
            return new Set<string>(activity_included.Keys);
        }

        public static SetContainer<Activity1> SelectedActivityNames()
        {
            return new SetContainer<Activity1>(activity_selected);
        }

        public static SetContainer<Activity1> ActivityList()
        {
            return new SetContainer<Activity1>(activities);
        }

        public static SetContainer<Document> DocumentList()
        {
            result = new SetContainer<Document>();

            foreach (Activity1 act1 in activities)
                if (activity_selected.Contains(act1))
                {
                    result = documents[act1];
                    return new SetContainer<Document>(result.ToArray());
                }

            return new SetContainer<Document>();

        }

        public static SetContainer<string> ActivitySelectionStatus()
        {
            return new SetContainer<string>(activity_selection_status_set);
        }

        [Rule]
        public static void edit_existing_plan()
        {
            Condition.IsTrue(Parameters.login_success);
            Condition.IsTrue(!Parameters.file_directory_status);
            Requires(activity_selected.Count == 0);
            Requires(activities.Count == 0);
            Requires(!plan_view_loaded_flag);
            plan_view_loaded_flag = true;
            activities = Parameters.acts;
        }

        //[Rule]
        //public static void plan_loaded(Activity act)
        //{
        //    Requires(!activities.Contains(act));
        //    Requires(plan_view_loaded_flag == true);
        //    activities.Add(act);
        //}

        [Rule]
        public static void select_activity([Domain("ActivityList")] Activity1 act)
        {
            Requires(!activity_selected.Contains(act));
            Requires(activities.Count > 0);
            changeSelection(act);
            flyout_loaded = true;
        }
        
        [Rule]
        public static void change_activity_selection([Domain("SelectedActivityNames")] Activity1 selected_acts, activity_selection_status ass)
        {            
            Requires(selection_changed_counter < Parameters.max_number_of_selection_changes);
            Activity1 activity_changed;
            activities = ChangeActivitySelectionStatus(selected_acts, ass, out activity_changed);
            activity_selected = new SetContainer<Activity1>();
            activity_selected.Add(activity_changed);
            selection_changed_counter++;
        }

        [Rule]
        public static void close_flyout([Domain("SelectedActivityNames")] Activity1 selected_acts)
        {
            Requires(flyout_loaded);
            activity_selected = new SetContainer<Activity1>();

            if (selection_changed_counter == Parameters.max_number_of_selection_changes)
            {
                activities = new SetContainer<Activity1>();
            }
        }

        [AcceptingStateCondition]
        public static bool accept_close_flyout()
        {
            return (activity_selected.Count == 0) && (activities.Count == 0);
        }

        public static void changeSelection(Activity1 newSelection)
        {
            if (activity_selected.Count > 0)
            {
                activity_selected = new SetContainer<Activity1>();
            }
            activity_selected.Add(newSelection);
        }

        //[Rule]
        //public static void include_activity([Domain("ActivityList")] Activity act)
        //{
        //    Requires(activities.Contains(act));
        //    activities = ChangeActivitySelectionStatus(act);
        //    BuildActivityDocumentsList(act, doc_selection_status.included);
        //}

        //[Rule]
        //public static void exclude_activity([Domain("ActivityList")] Activity act)
        //{
        //    Requires(activities.Contains(act));
        //    activities = ChangeActivitySelectionStatus(act, activity_selection_status.excluded);
        //    BuildActivityDocumentsList(act, doc_selection_status.excluded);
        //}


        //[Rule]
        //public static void change_document_selection([Domain("ActivityList")] Activity act, [Domain("DocumentList")] Document doc)
        //{
        //    Requires(activity_selected.Contains(act));
        //    documents[act] = ChangeDocumentSelectionStatus(doc.name, documents[act]);
        //    dialog_status_flag = true;

        //}

        //[Rule]
        //public static void dialog_status([Domain("ActivityList")] Activity act, [Domain("DocumentList")] Document doc)
        //{
        //    Requires(dialog_status_flag);
        //    dialog_status_flag = false;

        //}

        static void Requires(bool condition)
        {
            Condition.IsTrue(condition);
        }

        static doc_selection_status GetDocumentSelectionStatus(string doc_name, SetContainer<Document> docs)
        {
            foreach (Document d in docs)
            {
                if (doc_name == d.name)
                {
                    return d.status;
                }
            }
            return doc_selection_status.null_none;
        }

        static SetContainer<Document> ChangeDocumentSelectionStatus(string name, SetContainer<Document> docs)
        {
            SetContainer<Document> new_docs = new SetContainer<Document>();
            doc_selection_status current_value = GetDocumentSelectionStatus(name, docs);
            i = (int)current_value;
            if (i < 2)
            {
                i++;
            }
            else
            {
                i = 0;
            }

            new_doc_selection_status = (doc_selection_status)i;

            foreach (Document d in docs)
            {
                if (name == d.name)
                {
                    new_docs.Add(new Document(d.name, new_doc_selection_status, d.type));
                }
            }

            return new_docs;
        }

        //private static SetContainer<Activity> ChangeActivitySelectionStatus(Activity act)
        //{
        //    activity_selection_status current_value = act.status;
        //    i = (int)current_value;
        //    if (i < 3)
        //    {
        //        i++;
        //    }
        //    else
        //    {
        //        i = 0;
        //    }

        //    new_activity_selection_status = (activity_selection_status)i;

        //    return ChangeActivitySelectionStatus(act, new_activity_selection_status, out act_changed);
        //}

        private static SetContainer<Activity1> ChangeActivitySelectionStatus(Activity1 act, activity_selection_status ass, out Activity1 status_changed_act)
        {
            SetContainer<Activity1> new_activities = new SetContainer<Activity1>();
            status_changed_act = new Activity1(act.name, ass, act.isMandatory);

            foreach (Activity1 a in activities)
            {
                if (act.name == a.name)
                {
                    new_activities.Add(status_changed_act);
                }
            }

            return new_activities;
        }

        //private static SetContainer<Activity1> ChangeActivitySelectionSetStatus(Activity1 act, string ass, out Activity1 status_changed_act)
        //{
        //    SetContainer<Activity1> new_activities = new SetContainer<Activity1>();
        //    status_changed_act = new Activity1(act.name, ass, act.isMandatory);

        //    foreach (Activity1 a in activities)
        //    {
        //        if (act.name == a.name)
        //        {
        //            new_activities.Add(status_changed_act);
        //        }
        //    }

        //    return new_activities;
        //}

        static void BuildActivityDocumentsList(Activity1 act, doc_selection_status new_doc_selection_status)
        {
            if (act.isMandatory && !documents.Keys.Contains(act))
            {
                documents.Add(new KeyValuePair<Activity1, SetContainer<Document>>(act, new SetContainer<Document>
                    (new Document(doc1, new_doc_selection_status, document_type.mandatory))));
            }

        }


    }

    public struct Activity1
    {
        public string name;
        public activity_selection_status status;
        public bool isMandatory;

        public Activity1(string name, activity_selection_status status, bool isMandatory)
        {
            this.name = name;
            this.status = status;
            this.isMandatory = isMandatory;
        }

    }

    //public struct Activity
    //{
    //    public string name;
    //    public string activity_selection_status;
    //    public bool isMandatory;

    //    public Activity(string name, string activity_selection_status, bool isMandatory)
    //    {
    //        this.name = name;
    //        this.activity_selection_status = activity_selection_status;
    //        this.isMandatory = isMandatory;
    //    }

    //}



    public struct Document
    {
        public string name;
        public doc_selection_status status;
        public document_type type;

        public Document(string name, doc_selection_status status, document_type type)
        {
            this.name = name;
            this.status = status;
            this.type = type;
        }

    }

    public enum doc_selection_status
    {
        null_none = 0,
        included = 1,
        excluded = 2
    }

    public enum activity_selection_status
    {
        no_selection = 0,
        include = 1        
    }

    public enum document_type
    {
        mandatory,
        optional
    }

}

