using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Modeling;

namespace GuidedModelExample
{
    /// <summary>
    /// An example model program.
    /// </summary>
    public static class ModelProgram
    {
        public enum BacillAiState { not_started, logged_in, home_screen, 
            new_screening_screen, new_screening_screen_pending, camera_ready, phone_images_store,
            camera_done, review_and_send, add_pending, choose_from_library, choose_from_library_add_pending, submit, submit_notification_done, results_screen_pending, 
            image_sent, image_not_sent, tutorial_start, tutorial_done  };

        public static BacillAiState state = BacillAiState.not_started;

        public static string id = "1001A";

        public static bool submitted_flag = false;

        public static SetContainer<Patient> patients = new SetContainer<Patient>();
        public static SetContainer<Image> images = new SetContainer<Image>();
        public static SetContainer<Clinician> clinicians = new SetContainer<Clinician>();

        [Rule(Action = "login(clinician)")]
        static void login(Clinician clinician)
        {
            Condition.IsTrue(state == BacillAiState.not_started);
            Condition.IsNotNull(clinician);            

            state = BacillAiState.logged_in;

            clinicians.Add(clinician);

        }

        [Rule(Action = "logoff(clinician)")]
        static void logoff([Domain("clinicians")] Clinician clinician)
        {
            Condition.IsTrue(state == BacillAiState.logged_in);

            state = BacillAiState.not_started;

        }

        [Rule(Action = "home_screen()")]
        static void home_screen()
        {
            Condition.IsTrue(state == BacillAiState.logged_in);

            state = BacillAiState.home_screen;
        }

        [Rule(Action = "new_screening_main_button()")]
        static void new_screening_main_button()
        {
            Condition.IsTrue(state == BacillAiState.home_screen);

            state = BacillAiState.new_screening_screen;
        }

        [Rule(Action = "results_main_button()")]
        static void results_main_button()
        {
            Condition.IsTrue(state == BacillAiState.home_screen && submitted_flag == true);
            state = BacillAiState.results_screen_pending;
        }

        [Rule(Action = "tutorial_main_button()")]
        static void tutorial_main_button()
        {
            Condition.IsTrue(state == BacillAiState.home_screen);
            state = BacillAiState.tutorial_start;
        }

        //[Rule(Action = "results_toolbar_button()")]
        //static void results_toolbar_button()
        //{
            
        //}

        //[Rule(Action = "home_screen_toolbar_button()")]
        //static void home_screen_toolbar_button()
        //{
            
        //}

        //[Rule(Action = "settings_toolbar_button()")]
        //static void settings_toolbar_button()
        //{
            
        //}

        [Rule(Action = "new_screening_screen()")]
        static void new_screening_screen()
        {
            Condition.IsTrue(state == BacillAiState.new_screening_screen);

            Patient patient = new Patient();
            patient.Patient_id = id;
            patients.Add(patient);

            state = BacillAiState.new_screening_screen_pending;
        }

        [Rule(Action = "new_screening_done_button(patient)")]
        static void new_screening_done_button([Domain("patients")] Patient patient)
        {
            Condition.IsTrue(state == BacillAiState.new_screening_screen_pending);
            Condition.IsNotNull(patient);

            state = BacillAiState.camera_ready;
        }

        [Rule(Action = "take_image(image)")]
        static void take_image(Image image)
        {
            Condition.IsTrue(state == BacillAiState.camera_ready);
            Condition.IsNotNull(image);
            images.Add(image);

            state = BacillAiState.camera_done;

        }

        [Rule(Action = "navigate_to_images()")]
        static void navigate_to_images()
        {
            Condition.IsTrue(state == BacillAiState.camera_ready);

            state = BacillAiState.phone_images_store;
        }

        [Rule]
        static void phone_images_store()
        {
            Condition.IsTrue(state == BacillAiState.phone_images_store);
        }

        [Rule]
        static void camera_done_button()
        {
            Condition.IsTrue(state == BacillAiState.camera_done);

            state = BacillAiState.review_and_send;
        }

        [Rule(Action = "review_and_send_screen()")]
        static void review_and_send_screen()
        {
            Condition.IsTrue(state == BacillAiState.review_and_send);
            state = BacillAiState.add_pending;
        }

        [Rule(Action = "review_and_send_add_from_library_button()")]
        static void review_and_send_add_from_library_button()
        {
            Condition.IsTrue(state == BacillAiState.add_pending);
            state = BacillAiState.choose_from_library;
        }

        [Rule(Action = "review_and_send_submit_button()")]
        static void review_and_send_submit_button()
        {
            Condition.IsTrue(state == BacillAiState.review_and_send);
            state = BacillAiState.submit;
        }

        [Rule(Action = "choose_from_library_screen()")]
        static void choose_from_library_screen()
        {
            Condition.IsTrue(state == BacillAiState.choose_from_library);
            state = BacillAiState.choose_from_library_add_pending;
        }

        [Rule(Action = "choose_from_library_screen_add_button()")]
        static void choose_from_library_screen_add_button()
        {
            Condition.IsTrue(state == BacillAiState.choose_from_library_add_pending);
            state = BacillAiState.review_and_send;
        }

        [Rule(Action = "submitted_notification()")]
        static void submitted_notification()
        {
            Condition.IsTrue(state == BacillAiState.submit);
            state = BacillAiState.submit_notification_done;
        }

        [Rule(Action = "submitted_notification_done_button()")]
        static void submitted_notification_done_button()
        {
            Condition.IsTrue(state == BacillAiState.submit_notification_done);
            state = BacillAiState.home_screen;
            submitted_flag = true;
        }

        [Rule(Action = "results_screen(image)")]
        static void results_screen([Domain("images")] Image image)
        {
            Condition.IsTrue(state == BacillAiState.results_screen_pending);
            if (image.image_status == ImageStatus.completed)
            {
                state = BacillAiState.image_sent;
            }
            else
            {
                state = BacillAiState.image_not_sent;
            }
        }

        [Rule(Action = "images_sent(image)")]
        static void images_sent(Image image)
        {
            Condition.IsTrue(state == BacillAiState.image_sent);
            state = BacillAiState.home_screen;
        }

        //[Rule(Action = "tutorial_screen()")]
        //static void tutorial_screen()
        //{
        //    Condition.IsTrue(state == BacillAiState.tutorial_start);
        //    state = BacillAiState.tutorial_done;
        //}

        //[Rule(Action = "tutorial_screen_done_button()")]
        //static void tutorial_screen_done_button()
        //{
        //    Condition.IsTrue(state == BacillAiState.tutorial_done);
        //    state = BacillAiState.home_screen;
        //}
    }

    public enum ImageStatus { pending, completed};

    public struct Image
    {
        public ImageStatus image_status;
    }

    public class Result
    {

    }

    public struct Clinician
    {
        public string id;
        public string password;
    }

    public struct Patient
    {
        public string Patient_id;
    }


}
